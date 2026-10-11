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

