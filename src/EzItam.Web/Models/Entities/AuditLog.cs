using System.ComponentModel.DataAnnotations;

namespace EzItam.Web.Models.Entities;

public class AuditLog
{
    public long Id { get; set; }

    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;

    [StringLength(50)]
    public string EntityKey { get; set; } = string.Empty;

    public AuditAction Action { get; set; }

    /// <summary>변경 내용 (JSON: {속성: {Old, New}})</summary>
    public string? Changes { get; set; }

    [StringLength(100)]
    public string? UserName { get; set; }

    public DateTime Timestamp { get; set; }
}
