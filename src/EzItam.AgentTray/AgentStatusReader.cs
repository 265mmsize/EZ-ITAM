using System.Text.Json;

namespace EzItam.AgentTray;

/// <summary>EzItam.Agent 서비스가 쓰는 상태·이력 파일을 읽기만 합니다 (서비스와는 파일을 통해서만 연결됩니다).</summary>
public sealed class AgentStatusReader
{
    private readonly string _stateFile;
    private readonly string _historyFile;

    public AgentStatusReader(string? stateDirectory = null)
    {
        var dir = string.IsNullOrWhiteSpace(stateDirectory)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EzItam", "Agent")
            : stateDirectory;
        _stateFile = Path.Combine(dir, "agent-state.json");
        _historyFile = Path.Combine(dir, "agent-history.json");
    }

    public AgentSummary ReadSummary()
    {
        if (!File.Exists(_stateFile))
            return new AgentSummary(null, null, false);
        try
        {
            var state = JsonSerializer.Deserialize<StateView>(File.ReadAllText(_stateFile));
            return new AgentSummary(state?.AgentId, state?.LastInventoryUtc, state?.AgentId is not null);
        }
        catch (JsonException)
        {
            return new AgentSummary(null, null, false);
        }
    }

    public List<HistoryEntryView> ReadHistory()
    {
        if (!File.Exists(_historyFile)) return new();
        try
        {
            var list = JsonSerializer.Deserialize<List<HistoryEntryView>>(File.ReadAllText(_historyFile)) ?? new();
            list.Reverse(); // 최신 항목을 먼저 보여줍니다.
            return list;
        }
        catch (JsonException)
        {
            return new();
        }
    }

    private sealed class StateView
    {
        public int? AgentId { get; set; }
        public DateTime? LastInventoryUtc { get; set; }
    }
}

public sealed record AgentSummary(int? AgentId, DateTime? LastInventoryUtc, bool Enrolled);

public sealed record HistoryEntryView(DateTime TimestampUtc, string Kind, string Status, string Message);
