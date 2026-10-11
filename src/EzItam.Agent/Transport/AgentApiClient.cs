using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EzItam.Contracts;
using Microsoft.Extensions.Options;

namespace EzItam.Agent.Transport;

/// <summary>서버 인증 실패(토큰 폐기, 재등록 필요)를 알리는 예외</summary>
public sealed class AgentUnauthorizedException : Exception
{
    public bool DuringEnroll { get; }
    public AgentUnauthorizedException(bool duringEnroll = false) : base("서버가 에이전트 인증을 거부했습니다.")
        => DuringEnroll = duringEnroll;
}

public sealed class AgentApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public AgentApiClient(HttpClient http, IOptions<AgentOptions> options)
    {
        _http = http;
        var url = options.Value.ServerUrl.Trim();
        if (!url.EndsWith('/')) url += "/"; // 가상 디렉터리 아래에 배포해도 상대 경로가 올바르게 붙도록 합니다.
        _http.BaseAddress = new Uri(url);
        _http.Timeout = TimeSpan.FromSeconds(60);
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EzItamAgent", AgentOptions.AgentVersion));
    }

    public async Task<EnrollResponse> EnrollAsync(EnrollRequest request, CancellationToken ct)
    {
        using var resp = await _http.PostAsJsonAsync("api/agent/enroll", request, JsonOptions, ct);
        if (resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden) throw new AgentUnauthorizedException(duringEnroll: true);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<EnrollResponse>(JsonOptions, ct))!;
    }

    public async Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest request, string token, CancellationToken ct)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/agent/heartbeat")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var resp = await _http.SendAsync(msg, ct);
        if (resp.StatusCode is HttpStatusCode.Unauthorized) throw new AgentUnauthorizedException();
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<HeartbeatResponse>(JsonOptions, ct))!;
    }

    /// <summary>인벤토리는 gzip 으로 압축해 보냅니다. 실패 시 예외를 던지고, 다음 주기에 최신 값으로 다시 수집합니다.</summary>
    public async Task SendInventoryAsync(InventoryPayload payload, string token, CancellationToken ct)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);
        using var compressed = new MemoryStream();
        using (var gz = new GZipStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            gz.Write(json);
        compressed.Position = 0;

        using var content = new StreamContent(compressed);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        content.Headers.ContentEncoding.Add("gzip");

        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/agent/inventory") { Content = content };
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var resp = await _http.SendAsync(msg, ct);
        if (resp.StatusCode is HttpStatusCode.Unauthorized) throw new AgentUnauthorizedException();
        resp.EnsureSuccessStatusCode();
    }
}
