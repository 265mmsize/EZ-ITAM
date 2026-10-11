using System.ComponentModel.DataAnnotations;

namespace EzItam.Web.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "아이디를 입력하세요.")]
    [Display(Name = "아이디")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호를 입력하세요.")]
    [DataType(DataType.Password)]
    [Display(Name = "비밀번호")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "로그인 상태 유지")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "현재 비밀번호를 입력하세요.")]
    [DataType(DataType.Password)]
    [Display(Name = "현재 비밀번호")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "새 비밀번호를 입력하세요.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "비밀번호는 8자 이상이어야 합니다.")]
    [DataType(DataType.Password)]
    [Display(Name = "새 비밀번호")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "새 비밀번호 확인")]
    [Compare(nameof(NewPassword), ErrorMessage = "새 비밀번호가 일치하지 않습니다.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UserListItem
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public bool IsLockedOut { get; set; }
}

public class UserFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "아이디는 필수입니다.")]
    [StringLength(50)]
    [Display(Name = "아이디")]
    public string UserName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
    [Display(Name = "이메일")]
    public string? Email { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "비밀번호")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "권한을 선택하세요.")]
    [Display(Name = "권한")]
    public string Role { get; set; } = "Viewer";
}
