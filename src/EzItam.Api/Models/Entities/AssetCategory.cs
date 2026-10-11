using System.ComponentModel.DataAnnotations;

namespace EzItam.Api.Models.Entities;

public class AssetCategory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "코드는 필수입니다.")]
    [StringLength(10)]
    [Display(Name = "코드")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "이름은 필수입니다.")]
    [StringLength(100)]
    [Display(Name = "분류명")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "설명")]
    public string? Description { get; set; }

    [Display(Name = "감가상각 기간(월)")]
    [Range(0, 600)]
    public int DepreciationMonths { get; set; } = 48;

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
