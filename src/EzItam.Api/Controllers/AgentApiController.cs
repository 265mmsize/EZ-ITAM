using System.IO.Compression;
using System.Text.Json;

namespace EzItam.Api.Controllers;

/// <summary>PC 에이전트 전용 API. 에이전트가 먼저 접속하는 구조이며, 서버는 응답으로 주기를 제어합니다.</summary>
[ApiController]
[Route("api/agent")]
[AllowAnonymous]
public class AgentApiController : ControllerBase
{
    private const long MaxInventoryBytes = 5 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAgentService _agents;

    public AgentApiController(IAgentService agents) => _agents = agents;

    [HttpPost("enroll")]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest request, CancellationToken ct)
    {
        try { return Ok(await _agents.EnrollAsync(request, ct)); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest request, CancellationToken ct)
    {
        var agent = await _agents.AuthenticateAsync(BearerToken(), ct);
        if (agent is null) return Unauthorized();
        return Ok(await _agents.HeartbeatAsync(agent, request, ct));
    }

    [HttpPost("inventory")]
    [RequestSizeLimit(MaxInventoryBytes)]
    public async Task<IActionResult> Inventory(CancellationToken ct)
    {
        var agent = await _agents.AuthenticateAsync(BearerToken(), ct);
        if (agent is null) return Unauthorized();
        if (agent.Status != AgentStatus.Approved)
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "승인되지 않은 에이전트입니다." });

        var payload = await ReadPayloadAsync(ct);
        if (payload is null) return BadRequest(new { error = "인벤토리 본문을 읽을 수 없습니다." });
        if (payload.Hardware is null || payload.Software is null || string.IsNullOrWhiteSpace(payload.Hardware.HostName))
            return BadRequest(new { error = "hardware, software, hostName 은 필수입니다." });

        await _agents.ApplyInventoryAsync(agent, payload, ct);
        return Accepted(new { received = payload.Software.Count });
    }

    /// <summary>내 링의 최신 패키지를 내려줍니다. 인증과 승인된 장치만 받을 수 있습니다.</summary>
    [HttpGet("update-package")]
    public async Task<IActionResult> UpdatePackage([FromServices] IUpdateCatalog updates, CancellationToken ct)
    {
        var agent = await _agents.AuthenticateAsync(BearerToken(), ct);
        if (agent is null) return Unauthorized();
        if (agent.Status != AgentStatus.Approved) return StatusCode(StatusCodes.Status403Forbidden);
        var path = updates.GetPackagePath(agent.Ring);
        if (path is null || !System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, "application/zip", Path.GetFileName(path));
    }

    private string? BearerToken()
    {
        var header = Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header["Bearer ".Length..].Trim() : null;
    }

    private async Task<InventoryPayload?> ReadPayloadAsync(CancellationToken ct)
    {
        Stream body = Request.Body;
        if (Request.Headers.ContentEncoding.ToString().Contains("gzip", StringComparison.OrdinalIgnoreCase))
            body = new GZipStream(Request.Body, CompressionMode.Decompress);

        var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        long total = 0;
        try
        {
            int read;
            while ((read = await body.ReadAsync(chunk, ct)) > 0)
            {
                total += read;
                if (total > MaxInventoryBytes * 4) return null;
                await buffer.WriteAsync(chunk.AsMemory(0, read), ct);
            }
        }
        catch (InvalidDataException) { return null; }

        buffer.Position = 0;
        try { return await JsonSerializer.DeserializeAsync<InventoryPayload>(buffer, JsonOptions, ct); }
        catch (Exception ex) when (ex is JsonException or NotSupportedException) { return null; }
    }
}
