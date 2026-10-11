using System.ComponentModel.DataAnnotations;

namespace EzItam.Web.Models.Entities;

/// <summary>라이선스 할당 이력 (사용자 또는 장비에 할당)</summary>
public class LicenseAssignment
{
    public int Id { get; set; }

    public int SoftwareLicenseId { get; set; }
    public SoftwareLicense? SoftwareLicense { get; set; }

    [Display(Name = "장비")]
    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }

    [Display(Name = "사용자")]
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Display(Name = "할당일시")]
    public DateTime AssignedAt { get; set; }

    [Display(Name = "해제일시")]
    public DateTime? ReleasedAt { get; set; }

    [StringLength(100)]
    public string? AssignedBy { get; set; }

    [StringLength(500)]
    [Display(Name = "비고")]
    public string? Notes { get; set; }

    public bool IsOpen => ReleasedAt is null;
}
