using EzItam.Agent.Collectors;
using EzItam.Agent.Transport;
using EzItam.Contracts;
using Microsoft.Extensions.Options;

namespace EzItam.Agent;

/// <summary>
/// 에이전트가 먼저 서버에 접속합니다(아웃바운드 전용). 서버 응답의 주기에 맞춰 heartbeat 를 보내고,
/// 승인된 장비에서 인벤토리가 필요할 때만 수집·전송합니다. 실패하면 지수 백오프로 재시도합니다.
/// </summary>
public sealed class AgentWorker : BackgroundService
{
    private static readonly TimeSpan MinBackoff = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromHours(1);

    private readonly AgentOptions _options;
    private readonly AgentApiClient _api;
    private readonly AgentStateStore _store;
    private readonly IInventoryCollector _collector;
    private readonly AgentUpdater _updater;
    private readonly ILogger<AgentWorker> _logger;

    /// <summary>이번에 발견한 업데이트를 적용해도 되는 시각(무작위 지연 적용)</summary>
    private DateTime? _updateNotBefore;

    public AgentWorker(
        IOptions<AgentOptions> options,
        AgentApiClient api,
        AgentStateStore store,
        IInventoryCollector collector,
        AgentUpdater updater,
        ILogger<AgentWorker> logger)
    {
        _options = options.Value;
        _api = api;
        _store = store;
        _collector = collector;
        _updater = updater;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 서버 집중 방지를 위해 시작 시점을 무작위로 늦춥니다.
        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(5, 60)), stoppingToken);

        var backoff = MinBackoff;
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay;
            try
            {
                var state = _store.Load();
                if (state.AgentId is null || state.Token is null)
                {
                    await EnrollAsync(state, stoppingToken);
                    state = _store.Load();
                }

                var hb = await _api.HeartbeatAsync(
                    new HeartbeatRequest(AgentOptions.AgentVersion, BootTimeUtc(), false, null),
                    state.Token!, stoppingToken);

                if (hb.Status != "Approved")
                {
                    _logger.LogInformation("서버 상태: {Status}. 승인 전까지 인벤토리를 보내지 않습니다.", hb.Status);
                }
                else if (hb.InventoryDue || IsInventoryStale(state))
                {
                    await SendInventoryAsync(state, stoppingToken);
                }

                await MaybeUpdateAsync(state, hb, stoppingToken);

                delay = TimeSpan.FromSeconds(hb.NextHeartbeatSeconds * Random.Shared.NextDouble() * 0.4 + hb.NextHeartbeatSeconds * 0.8);
                backoff = MinBackoff;
            }
            catch (AgentUnauthorizedException ex) when (!stoppingToken.IsCancellationRequested)
            {
                // 등록 단계에서 거부되면(키 불일치, 사용 중지된 장치) 짧게 재시도해도 소용없으므로 최대 간격으로 대기합니다.
                if (ex.DuringEnroll)
                {
                    _logger.LogWarning("서버가 등록을 거부했습니다. 등록 키 또는 장치 상태를 확인하세요. {Minutes}분 후 재시도합니다.", MaxBackoff.TotalMinutes);
                    delay = MaxBackoff;
                }
                else
                {
                    _logger.LogWarning("인증 실패. 토큰을 지우고 다시 등록합니다.");
                    _store.Clear();
                    delay = MinBackoff;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // 수집·저장·네트워크 등 어떤 오류도 서비스 전체를 멈추지 않도록 삼키고 백오프합니다.
                _logger.LogWarning(ex, "주기 작업 실패. {Seconds}초 후 재시도합니다.", backoff.TotalSeconds);
                delay = backoff;
                backoff = TimeSpan.FromTicks(Math.Min(backoff.Ticks * 2, MaxBackoff.Ticks));
            }

            await Task.Delay(delay, stoppingToken);
        }
    }

    /// <summary>서버가 새 버전을 알렸을 때만 적용합니다. 적용 시점은 무작위로 늦춰 장치들이 한꺼번에 재시작하지 않게 합니다.</summary>
    private async Task MaybeUpdateAsync(AgentState state, HeartbeatResponse hb, CancellationToken ct)
    {
        if (hb.TargetVersion is null || hb.UpdateSha256 is null)
        {
            _updateNotBefore = null;
            return;
        }
        _updateNotBefore ??= DateTime.UtcNow.AddMinutes(Random.Shared.NextDouble() * _options.UpdateJitterMinutes);
        if (DateTime.UtcNow < _updateNotBefore) return;

        _logger.LogInformation("새 버전 {Target} 적용을 시작합니다 (현재 {Current}).", hb.TargetVersion, AgentOptions.AgentVersion);
        await _updater.TryApplyAsync(state, hb.TargetVersion, hb.UpdateSha256, ct);
    }

    private async Task EnrollAsync(AgentState state, CancellationToken ct)
    {
        var hw = await _collector.CollectHardwareAsync(ct);
        EnrollResponse resp;
        try
        {
            resp = await _api.EnrollAsync(new EnrollRequest(
            EnrollmentKey: _options.EnrollmentKey,
            DeviceUuid: hw.DeviceUuid,
            SerialNumber: hw.SerialNumber,
            HostName: hw.HostName,
            Manufacturer: hw.Manufacturer,
            Model: hw.Model,
            OsName: hw.OsName,
            OsVersion: hw.OsVersion,
            MacAddresses: hw.MacAddresses,
            IpAddress: hw.IpAddress,
            AgentVersion: AgentOptions.AgentVersion), ct);
        }
        catch (AgentUnauthorizedException)
        {
            throw new AgentUnauthorizedException(duringEnroll: true);
        }

        state.AgentId = resp.AgentId;
        state.Token = resp.Token;
        state.LastInventoryUtc = null;
        _store.Save(state);
        _logger.LogInformation("등록 완료. AgentId={AgentId}, 상태={Status}", resp.AgentId, resp.Status);
    }

    private async Task SendInventoryAsync(AgentState state, CancellationToken ct)
    {
        var hw = await _collector.CollectHardwareAsync(ct);
        var sw = await _collector.CollectSoftwareAsync(ct);
        await _api.SendInventoryAsync(new InventoryPayload(DateTime.UtcNow, hw, sw), state.Token!, ct);

        state.LastInventoryUtc = DateTime.UtcNow;
        _store.Save(state);
        _logger.LogInformation("인벤토리 전송 완료: 설치 SW {Count}건", sw.Count);
    }

    private bool IsInventoryStale(AgentState state)
        => state.LastInventoryUtc is null
           || state.LastInventoryUtc < DateTime.UtcNow.AddHours(-_options.InventoryHours);

    private static DateTime? BootTimeUtc()
        => DateTime.UtcNow - TimeSpan.FromMilliseconds(Environment.TickCount64);
}
