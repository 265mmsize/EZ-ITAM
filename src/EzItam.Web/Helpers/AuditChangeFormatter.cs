using System.Text.Json;

namespace EzItam.Web.Helpers;

public static class AuditChangeFormatter
{
    public record Line(string Property, string? Old, string? New);

    /// <summary>AuditDto.Changes JSON 을 "속성: 이전 → 이후" 목록으로 변환합니다.</summary>
    public static IReadOnlyList<Line> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<Line>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var list = new List<Line>();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                string? oldV = prop.Value.TryGetProperty("Old", out var o) ? Render(o) : null;
                string? newV = prop.Value.TryGetProperty("New", out var n) ? Render(n) : null;
                list.Add(new Line(prop.Name, oldV, newV));
            }
            return list;
        }
        catch (JsonException)
        {
            return new[] { new Line("raw", null, json) };
        }
    }

    private static string? Render(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.String when DateTime.TryParse(e.GetString(), out var d) => d.TimeOfDay == TimeSpan.Zero ? d.ToString("yyyy-MM-dd") : d.ToString("yyyy-MM-dd HH:mm"),
        JsonValueKind.String => e.GetString(),
        _ => e.ToString()
    };
}
