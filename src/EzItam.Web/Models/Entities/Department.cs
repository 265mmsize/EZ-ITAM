using System.ComponentModel.DataAnnotations;

namespace EzItam.Web.Models.Entities;

public class Department
{
    public int Id { get; set; }

    [Required(ErrorMessage = "부서코드는 필수입니다.")]
    [StringLength(20)]
    [Display(Name = "부서코드")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "부서명은 필수입니다.")]
    [StringLength(100)]
    [Display(Name = "부서명")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Oracle EBS HR_ALL_ORGANIZATION_UNITS.ORGANIZATION_ID</summary>
    [Display(Name = "ERP 조직 ID")]
    public long? ErpOrganizationId { get; set; }

    [Display(Name = "사용")]
    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
