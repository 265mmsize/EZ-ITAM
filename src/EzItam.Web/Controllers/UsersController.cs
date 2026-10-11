using EzItam.Web.Api;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class UsersController : BaseController
{
    public UsersController(ItamApiClient api) : base(api) { }

    public async Task<IActionResult> Index() => View(await Api.ListUsersAsync());

    public IActionResult Create()
    {
        PopulateRoles();
        return View("Form", new UserFormDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Password)) ModelState.AddModelError(nameof(model.Password), "비밀번호를 입력하세요.");
        if (ModelState.IsValid)
        {
            try
            {
                var u = await Api.CreateUserAsync(model);
                Success($"사용자 {u.UserName} 이(가) 생성되었습니다.");
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        PopulateRoles();
        return View("Form", model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = (await Api.ListUsersAsync()).FirstOrDefault(u => u.Id == id);
        if (user is null) return NotFound();
        PopulateRoles();
        return View("Form", new UserFormDto { Id = user.Id, UserName = user.UserName, Email = user.Email, Role = user.Roles.FirstOrDefault() ?? "Viewer" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserFormDto model)
    {
        if (id != model.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await Api.UpdateUserAsync(id, model);
                Success("사용자 정보가 저장되었습니다.");
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        PopulateRoles();
        return View("Form", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await Api.DeleteUserAsync(id);
            Success("사용자가 삭제되었습니다.");
        }
        catch (ApiException ex) { Error(ex.Message); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id)
    {
        await Api.UnlockUserAsync(id);
        Success("계정 잠금이 해제되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    private void PopulateRoles()
    {
        ViewBag.Roles = new List<SelectListItem>
        {
            new("관리자 (Admin) - 모든 기능", "Admin"),
            new("담당자 (Manager) - 등록/수정/불출/반납", "Manager"),
            new("조회자 (Viewer) - 조회만", "Viewer")
        };
    }
}
