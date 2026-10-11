namespace EzItam.Web.Localization;

/// <summary>DI 가 없는 곳(헬퍼, 열거형 표시, 검증 메시지)에서 같은 번역 저장소를 씁니다. 시작 시 한 번 연결합니다.</summary>
public static class Strings
{
    public static JsonLocalizerStore? Store { get; set; }

    public static string Get(string key, params object[] args)
        => Store is null ? (args.Length == 0 ? key : string.Format(key, args)) : Store.Get(key, args);
}
