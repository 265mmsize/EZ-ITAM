using EzItam.Web.Api;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

public class VendorsController : BaseController
{
    public VendorsController(ItamApiClient api) : base(api) { }

    public async Task<IActionResult> Index() => View(await Api.ListAsync<VendorDto>("vendors"));

    [Authorize(Policy = "CanEdit")]
    public IActionResult Create() => View("Form", new VendorDto());

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VendorDto model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        try
        {
            await Api.CreateAsync("vendors", model);
            Success("공급업체가 등록되었습니다.");
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) { AddApiError(ex); return View("Form", model); }
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id) => View("Form", await Api.GetAsync<VendorDto>("vendors", id));

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VendorDto model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", model);
        try
        {
            await Api.UpdateAsync("vendors", id, model);
            Success("공급업체가 저장되었습니다.");
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) { AddApiError(ex); return View("Form", model); }
    }

    [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await Api.DeleteAsync("vendors", id);
            Success("공급업체가 삭제되었습니다. (연결된 자산의 공급업체는 비워집니다.)");
        }
        catch (ApiException ex) { Error(ex.Message); }
        return RedirectToAction(nameof(Index));
    }
}
