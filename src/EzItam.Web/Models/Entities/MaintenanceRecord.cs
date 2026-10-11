using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EzItam.Web.Models.Entities;

public class MaintenanceRecord
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    [Display(Name = "유형")]
    public MaintenanceType Type { get; set; }

    [Required(ErrorMessage = "작업일은 필수입니다.")]
    [Display(Name = "작업일")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "내용은 필수입니다.")]
    [StringLength(1000)]
    [Display(Name = "내용")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "비용")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 999999999999)]
    public decimal? Cost { get; set; }

    [Display(Name = "업체")]
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    [Display(Name = "완료일")]
    [DataType(DataType.Date)]
    public DateTime? CompletedAt { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public bool IsCompleted => CompletedAt.HasValue;
}
