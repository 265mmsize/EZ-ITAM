using System.Text.Json;
using EzItam.Contracts;

namespace EzItam.Api.Services;

/// <summary>
/// 링별 최신 패키지 정보를 읽습니다. 배포 폴더 구조: {Updates:Root}/{Ring}/manifest.json 과 패키지 zip.
/// 관리자는 새 zip 과 manifest.json 을 폴더에 복사하는 것만으로 배포합니다.
/// </summary>
public interface IUpdateCatalog
{
    UpdateManifest? GetManifest(AgentRing ring);
    string? GetPackagePath(AgentRing ring);
}

public sealed class UpdateCatalog : IUpdateCatalog
{
    private readonly string _root;

    public UpdateCatalog(string root) => _root = root;

    public UpdateManifest? GetManifest(AgentRing ring)
    {
        var path = Path.Combine(_root, ring.ToString(), "manifest.json");
        if (!File.Exists(path)) return null;
        try
        {
            var m = JsonSerializer.Deserialize<UpdateManifest>(File.ReadAllText(path), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (m is null || string.IsNullOrWhiteSpace(m.Version) || string.IsNullOrWhiteSpace(m.FileName)) return null;
            // 파일 이름에 경로 구분자가 있으면 거부합니다 (경로 탈출 방지).
            if (m.FileName.Contains('/') || m.FileName.Contains('\\') || m.FileName.Contains("..")) return null;
            return m;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public string? GetPackagePath(AgentRing ring)
    {
        var m = GetManifest(ring);
        return m is null ? null : Path.Combine(_root, ring.ToString(), m.FileName);
    }
}
