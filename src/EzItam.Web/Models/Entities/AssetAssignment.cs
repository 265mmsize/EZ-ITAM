using System.ComponentModel.DataAnnotations;

namespace EzItam.Web.Models.Entities;

/// <summary>자산 불출/반납 이력</summary>
public class AssetAssignment
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    [Display(Name = "사용자")]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Display(Name = "불출일시")]
    public DateTime AssignedAt { get; set; }

    [Display(Name = "반납일시")]
    public DateTime? ReturnedAt { get; set; }

    [StringLength(100)]
    [Display(Name = "처리자")]
    public string? AssignedBy { get; set; }

    [StringLength(100)]
    public string? ReturnedBy { get; set; }

    [StringLength(500)]
    [Display(Name = "비고")]
    public string? Notes { get; set; }

    public bool IsOpen => ReturnedAt is null;
}
