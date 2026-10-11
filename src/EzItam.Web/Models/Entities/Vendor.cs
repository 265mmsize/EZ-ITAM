using System.ComponentModel.DataAnnotations;

namespace EzItam.Web.Models.Entities;

public class Vendor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "공급업체명은 필수입니다.")]
    [StringLength(100)]
    [Display(Name = "공급업체명")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "담당자")]
    public string? ContactName { get; set; }

    [StringLength(30)]
    [Display(Name = "전화번호")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
    [StringLength(200)]
    [Display(Name = "이메일")]
    public string? Email { get; set; }

    [StringLength(500)]
    [Display(Name = "비고")]
    public string? Notes { get; set; }

    /// <summary>Oracle EBS AP_SUPPLIERS.VENDOR_ID</summary>
    [Display(Name = "ERP 공급업체 ID")]
    public long? ErpVendorId { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<SoftwareLicense> Licenses { get; set; } = new List<SoftwareLicense>();
}
