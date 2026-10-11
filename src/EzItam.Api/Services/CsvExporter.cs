using System.Text;

namespace EzItam.Api.Services;

public static class CsvExporter
{
    /// <summary>Excel에서 한글이 깨지지 않도록 UTF-8 BOM을 포함한 CSV 바이트를 생성합니다.</summary>
    public static byte[] Build<T>(IEnumerable<T> rows, params (string Header, Func<T, object?> Selector)[] columns)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", columns.Select(c => Escape(c.Header))));
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", columns.Select(c => Escape(Format(c.Selector(row))))));
        }
        var bom = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        return bom.Concat(body).ToArray();
    }

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        DateTime d => d.TimeOfDay == TimeSpan.Zero ? d.ToString("yyyy-MM-dd") : d.ToString("yyyy-MM-dd HH:mm"),
        decimal m => m.ToString("0.##"),
        bool b => b ? "Y" : "N",
        Enum e => EnumDisplay.Name(e),
        _ => value.ToString() ?? string.Empty
    };

    private static string Escape(string s)
    {
        if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }
}
