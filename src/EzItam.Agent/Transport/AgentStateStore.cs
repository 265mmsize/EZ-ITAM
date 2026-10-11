using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace EzItam.Agent.Transport;

/// <summary>등록 정보(서버가 준 AgentId, 토큰)와 마지막 인벤토리 시각을 저장합니다. Windows 에서는 토큰을 DPAPI(LocalMachine)로 보호합니다.</summary>
public sealed class AgentStateStore
{
    private readonly string _path;
    private readonly ILogger<AgentStateStore> _logger;

    public AgentStateStore(IOptions<AgentOptions> options, ILogger<AgentStateStore> logger)
    {
        var dir = options.Value.ResolveStateDirectory();
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "agent-state.json");
        _logger = logger;
    }

    public AgentState Load()
    {
        if (!File.Exists(_path)) return new AgentState();
        try
        {
            var stored = JsonSerializer.Deserialize<StoredState>(File.ReadAllText(_path)) ?? new StoredState();
            return new AgentState
            {
                AgentId = stored.AgentId,
                Token = stored.TokenProtected is null ? null : Unprotect(stored.TokenProtected),
                LastInventoryUtc = stored.LastInventoryUtc,
                LastUpdateAttemptVersion = stored.LastUpdateAttemptVersion
            };
        }
        catch (Exception ex) when (ex is JsonException or CryptographicException or IOException)
        {
            _logger.LogWarning(ex, "상태 파일을 읽을 수 없어 새로 등록합니다.");
            return new AgentState();
        }
    }

    public void Save(AgentState state)
    {
        var stored = new StoredState
        {
            AgentId = state.AgentId,
            TokenProtected = state.Token is null ? null : Protect(state.Token),
            LastInventoryUtc = state.LastInventoryUtc,
            LastUpdateAttemptVersion = state.LastUpdateAttemptVersion
        };
        File.WriteAllText(_path, JsonSerializer.Serialize(stored));
    }

    public void Clear() => Save(new AgentState());

    private static string Protect(string plain)
    {
        var bytes = Encoding.UTF8.GetBytes(plain);
        return Convert.ToBase64String(OperatingSystem.IsWindows() ? ProtectWindows(bytes) : bytes);
    }

    private static string Unprotect(string protectedText)
    {
        var bytes = Convert.FromBase64String(protectedText);
        return Encoding.UTF8.GetString(OperatingSystem.IsWindows() ? UnprotectWindows(bytes) : bytes);
    }

    [SupportedOSPlatform("windows")]
    private static byte[] ProtectWindows(byte[] data)
        => System.Security.Cryptography.ProtectedData.Protect(data, Entropy, DataProtectionScope.LocalMachine);

    [SupportedOSPlatform("windows")]
    private static byte[] UnprotectWindows(byte[] data)
        => System.Security.Cryptography.ProtectedData.Unprotect(data, Entropy, DataProtectionScope.LocalMachine);

    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("EzItamAgent.v1");

    private sealed class StoredState
    {
        public int? AgentId { get; set; }
        public string? TokenProtected { get; set; }
        public DateTime? LastInventoryUtc { get; set; }
        public string? LastUpdateAttemptVersion { get; set; }
    }
}

public sealed class AgentState
{
    public int? AgentId { get; set; }
    public string? Token { get; set; }
    public DateTime? LastInventoryUtc { get; set; }

    /// <summary>마지막으로 적용을 시도한 버전. 같은 버전은 실패해도 다시 시도하지 않습니다.</summary>
    public string? LastUpdateAttemptVersion { get; set; }
}
