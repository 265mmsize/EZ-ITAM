using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

public class EmployeesController : BaseController
{
    public EmployeesController(ItamApiClient api, IStringLocalizer<SharedResource> loc) : base(api, loc) { }

    public async Task<IActionResult> Index(string? search, int? departmentId, bool includeInactive)
    {
        ViewBag.Search = search;
        ViewBag.DepartmentId = departmentId;
        ViewBag.IncludeInactive = includeInactive;
        ViewBag.Departments = await OptionsAsync("departments", departmentId?.ToString());
        return View(await Api.ListAsync<EmployeeDto>("employees", new { search, departmentId, includeInactive }));
    }

    public async Task<IActionResult> Details(int id) => View(await Api.GetAsync<EmployeeDto>("employees", id));

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = await OptionsAsync("departments");
        return View("Form", new EmployeeDto());
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeDto model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var saved = await Api.CreateAsync("employees", model);
                Success(L["사원이 등록되었습니다."]);
                return RedirectToAction(nameof(Details), new { id = saved.Id });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        ViewBag.Departments = await OptionsAsync("departments", model.DepartmentId?.ToString());
        return View("Form", model);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await Api.GetAsync<EmployeeDto>("employees", id);
        ViewBag.Departments = await OptionsAsync("departments", item.DepartmentId?.ToString());
        return View("Form", item);
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeDto model)
    {
        if (id != model.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await Api.UpdateAsync("employees", id, model);
                Success(L["사원 정보가 저장되었습니다."]);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        ViewBag.Departments = await OptionsAsync("departments", model.DepartmentId?.ToString());
        return View("Form", model);
    }

    [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await Api.DeleteAsync("employees", id);
            Success(L["사원이 삭제되었습니다."]);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            Error(L[ex.Message]);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
