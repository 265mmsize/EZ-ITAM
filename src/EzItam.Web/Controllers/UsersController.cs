using EzItam.Web.Data;
using EzItam.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class UsersController : BaseController
{
    private readonly UserManager<IdentityUser> _userManager;
    public UsersController(UserManager<IdentityUser> userManager) => _userManager = userManager;

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
        var list = new List<UserListItem>();
        foreach (var u in users)
        {
            list.Add(new UserListItem
            {
                Id = u.Id, UserName = u.UserName ?? "", Email = u.Email,
                Roles = await _userManager.GetRolesAsync(u),
                IsLockedOut = await _userManager.IsLockedOutAsync(u)
            });
        }
        return View(list);
    }

    public IActionResult Create()
    {
        PopulateRoles();
        return View("Form", new UserFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "비밀번호를 입력하세요.");
        if (!ModelState.IsValid) { PopulateRoles(); return View("Form", model); }

        var user = new IdentityUser { UserName = model.UserName.Trim(), Email = model.Email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            PopulateRoles();
            return View("Form", model);
        }
        await _userManager.AddToRoleAsync(user, model.Role);
        Success($"사용자 {user.UserName} 이(가) 생성되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        PopulateRoles();
        return View("Form", new UserFormViewModel { Id = user.Id, UserName = user.UserName ?? "", Email = user.Email, Role = roles.FirstOrDefault() ?? AppRoles.Viewer });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) { PopulateRoles(); return View("Form", model); }
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.Email = model.Email;
        if (!string.Equals(user.UserName, model.UserName, StringComparison.Ordinal))
            await _userManager.SetUserNameAsync(user, model.UserName.Trim());
        await _userManager.UpdateAsync(user);

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(model.Role))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, token, model.Password);
            if (!reset.Succeeded)
            {
                foreach (var e in reset.Errors) ModelState.AddModelError(string.Empty, e.Description);
                PopulateRoles();
                return View("Form", model);
            }
        }
        Success("사용자 정보가 저장되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (string.Equals(user.UserName, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
        {
            Error("현재 로그인한 계정은 삭제할 수 없습니다.");
            return RedirectToAction(nameof(Index));
        }
        await _userManager.DeleteAsync(user);
        Success("사용자가 삭제되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        Success("계정 잠금이 해제되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    private void PopulateRoles()
    {
        ViewBag.Roles = new List<SelectListItem>
        {
            new("관리자 (Admin) - 모든 기능", AppRoles.Admin),
            new("담당자 (Manager) - 등록/수정/불출/반납", AppRoles.Manager),
            new("조회자 (Viewer) - 조회만", AppRoles.Viewer)
        };
    }
}
