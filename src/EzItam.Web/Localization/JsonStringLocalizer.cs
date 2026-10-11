using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace EzItam.Web.Localization;

/// <summary>Resources/Strings.{culture}.json 을 읽어 문자열을 돌려줍니다. 없는 키는 기본 언어(ko)를, 그것도 없으면 키를 그대로 씁니다.</summary>
public sealed class JsonLocalizerStore
{
    public const string DefaultCulture = "ko";
    private readonly Dictionary<string, Dictionary<string, string>> _tables = new(StringComparer.OrdinalIgnoreCase);

    public JsonLocalizerStore(IWebHostEnvironment env)
    {
        foreach (var culture in new[] { "ko", "en", "vi" })
        {
            var path = Path.Combine(env.ContentRootPath, "Resources", $"Strings.{culture}.json");
            if (!File.Exists(path)) continue;
            _tables[culture] = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path)) ?? new();
        }
    }

    public string Get(string key, params object[] args)
    {
        var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var value = Lookup(lang, key) ?? Lookup(DefaultCulture, key) ?? key;
        return args.Length == 0 ? value : string.Format(CultureInfo.CurrentUICulture, value, args);
    }

    private string? Lookup(string culture, string key)
        => _tables.TryGetValue(culture, out var t) && t.TryGetValue(key, out var v) ? v : null;
}

public sealed class JsonStringLocalizer<T> : IStringLocalizer<T>
{
    private readonly JsonLocalizerStore _store;
    public JsonStringLocalizer(JsonLocalizerStore store) => _store = store;

    public LocalizedString this[string name]
    {
        get { var v = _store.Get(name); return new LocalizedString(name, v, resourceNotFound: v == name); }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get { var v = _store.Get(name, arguments); return new LocalizedString(name, v, resourceNotFound: v == name); }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Enumerable.Empty<LocalizedString>();
}

/// <summary>화면 공통 문자열 리소스 타입</summary>
public sealed class SharedResource { }
