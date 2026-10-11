using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EzItam.Web.Models.Entities;

public class SoftwareLicense
{
    public int Id { get; set; }

    [Required(ErrorMessage = "소프트웨어명은 필수입니다.")]
    [StringLength(200)]
    [Display(Name = "소프트웨어명")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "제조사")]
    public string? Publisher { get; set; }

    [StringLength(50)]
    [Display(Name = "버전")]
    public string? Version { get; set; }

    [Display(Name = "라이선스 유형")]
    public LicenseType LicenseType { get; set; }

    [StringLength(500)]
    [Display(Name = "라이선스 키")]
    public string? LicenseKey { get; set; }

    [Display(Name = "보유 수량")]
    [Range(0, 1000000)]
    public int TotalSeats { get; set; } = 1;

    [Display(Name = "구매일")]
    [DataType(DataType.Date)]
    public DateTime? PurchaseDate { get; set; }

    [Display(Name = "만료일")]
    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "공급업체")]
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    [Display(Name = "구매비용")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 999999999999)]
    public decimal? Cost { get; set; }

    [StringLength(2000)]
    [Display(Name = "비고")]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<LicenseAssignment> Assignments { get; set; } = new List<LicenseAssignment>();

    [NotMapped]
    public int UsedSeats => Assignments.Count(a => a.ReleasedAt is null);

    [NotMapped]
    public int AvailableSeats => LicenseType == LicenseType.Site || LicenseType == LicenseType.Free
        ? int.MaxValue
        : TotalSeats - UsedSeats;

    [NotMapped]
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.Today;

    [NotMapped]
    public bool IsExpiringSoon => ExpiryDate.HasValue
        && ExpiryDate.Value.Date >= DateTime.Today
        && ExpiryDate.Value.Date <= DateTime.Today.AddDays(30);
}
