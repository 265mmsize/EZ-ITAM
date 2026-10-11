using System.Text.Json;
using Microsoft.Extensions.Options;

namespace EzItam.Agent.Transport;

/// <summary>최근 수집·통신 이력을 파일에 보관합니다. 트레이 UI 가 같은 파일을 읽어 사용자에게 보여줍니다.</summary>
public sealed class AgentHistoryStore
{
    private const int MaxEntries = 200;

    private readonly string _path;
    private readonly object _lock = new();

    public AgentHistoryStore(IOptions<AgentOptions> options)
    {
        var dir = options.Value.ResolveStateDirectory();
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "agent-history.json");
    }

    public void Add(string kind, string status, string message)
    {
        lock (_lock)
        {
            var list = LoadInternal();
            list.Add(new HistoryEntry(DateTime.UtcNow, kind, status, message));
            if (list.Count > MaxEntries)
                list.RemoveRange(0, list.Count - MaxEntries);
            File.WriteAllText(_path, JsonSerializer.Serialize(list));
        }
    }

    private List<HistoryEntry> LoadInternal()
    {
        if (!File.Exists(_path)) return new();
        try
        {
            return JsonSerializer.Deserialize<List<HistoryEntry>>(File.ReadAllText(_path)) ?? new();
        }
        catch (JsonException)
        {
            return new();
        }
    }
}

public sealed record HistoryEntry(DateTime TimestampUtc, string Kind, string Status, string Message);
