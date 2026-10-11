using System.ComponentModel.DataAnnotations;

namespace EzItam.Web.ViewModels;

public class AssetIndexViewModel
{
    public AssetFilter Filter { get; set; } = new();
    public PageResult<AssetDto> Result { get; set; } = new();
}

public class CheckOutViewModel
{
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;

    [Required(ErrorMessage = "사용자를 선택하세요.")]
    [Display(Name = "사용자")]
    public int? EmployeeId { get; set; }

    [StringLength(500)]
    [Display(Name = "비고")]
    public string? Notes { get; set; }
}

public class CheckInViewModel
{
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? CurrentEmployeeName { get; set; }

    [Display(Name = "반납 후 상태")]
    public AssetStatus NewStatus { get; set; } = AssetStatus.InStock;

    [Display(Name = "보관 위치")]
    public int? LocationId { get; set; }

    [StringLength(500)]
    [Display(Name = "비고")]
    public string? Notes { get; set; }
}

public class ChangeStatusViewModel
{
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public AssetStatus CurrentStatus { get; set; }

    [Display(Name = "변경할 상태")]
    public AssetStatus NewStatus { get; set; }

    [StringLength(500)]
    [Display(Name = "사유")]
    public string? Notes { get; set; }
}

public class LicenseAssignViewModel
{
    public int LicenseId { get; set; }
    public string LicenseName { get; set; } = string.Empty;

    [Display(Name = "사용자")]
    public int? EmployeeId { get; set; }

    [Display(Name = "장비")]
    public int? AssetId { get; set; }

    [StringLength(500)]
    [Display(Name = "비고")]
    public string? Notes { get; set; }
}
