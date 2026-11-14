using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace EzItam.Web.Controllers
{
    public class InventoryController(ItamApiClient api, IStringLocalizer<SharedResource> loc) : BaseController(api, loc)
    {
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var inventories = await Api.ListInventoryAsync(ct);
            return View(inventories);
        }

        [Authorize(Policy = "CanEdit")]
        public IActionResult Create()
        {
            {
                return View("Form", new InventoryDto());
            }
        }


        [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryDto model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            try
            {
                await Api.CreateInventoryAsync(model, ct);
                Success(L["inv.create.successfully"]);
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                AddApiError(ex);
                return View("Form", model);
            }
        }

        [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            try
            {
                var model = await Api.GetInventoryByIdAsync(id, ct);
                return View("Form", model);
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }

        [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryDto model, CancellationToken ct)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View("Form", model);
            try
            {
                await Api.UpdateInventoryAsync(id, model, ct);
                Success(L["inv.update.successfully"]);
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                AddApiError(ex);
                return View("Form", model);
            }
        }

        [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                await Api.DeleteInventoryAsync(id, ct);
                Success(L["inv.delete.successfully"]);
            }
            catch (ApiException ex)
            {
                Error(L[ex.Message]);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> Genbook(int id, CancellationToken ct)
        {
            try
            {
                await Api.GenInventoryBookAsync(id, ct);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ApiException ex)
            {
                Error(L[ex.Message]);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            try
            {
                var inventories = await Api.GetInventoryDetailByInventoryIdAsync(id, ct);
                return View(inventories);
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }

        [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> Scan(int id, CancellationToken ct)
        {
            InventoryDto inventory;
            try
            {
                inventory = await Api.GetInventoryByIdAsync(id, ct);
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            SetViewBagStatus();
            await SetViewBagLocation();
            await SetViewBagEmployee();

            return View(inventory);
        }

        [HttpGet]
        [Authorize(Policy = "CanEdit")]
        public async Task<ObjectResult> GetDetailByTag(int inventoryId, string tag, CancellationToken ct)
        {
            var detailByTag = await Api.GetAssetByInventoryIdAndTag(inventoryId, tag, ct);
            if (detailByTag is null || detailByTag.Id == 0)
            {
                return Ok(new { success = false, message = L["inv.scan.invalid"].Value });
            }
            return Ok(new { success = true, item = detailByTag });
        }

        [HttpPost]
        [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> SaveScanResult([FromBody] ScanResultRequest req, CancellationToken ct)
        {
            try
            {
                var res = await Api.PostScanResult(req, ct);
                return Ok(new { success = res });
            }
            catch (ApiException ex)
            {
                return StatusCode((int)ex.StatusCode, new { success = false, message = ex.Message });
            }
        }

        [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> PostDiff(int id, CancellationToken ct)
        {
            try
            {
                await Api.PostDiff(id, ct);
                Success(L["inv.postdiff.successfully"]);
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                Error(L[ex.Message]);
            }
            return RedirectToAction(nameof(Details), new {id});
        }



        public void SetViewBagStatus(string? selectedValue = null)
        {
            var selectList = Enum.GetValues(typeof(AssetStatus))
                .Cast<AssetStatus>()
                .Select(status => new SelectListItem
                {
                    Value = ((int)status).ToString(),
                    Text = status.ToString(),
                    Selected = selectedValue != null && ((int)status).ToString() == selectedValue
                })
                .ToList();

            ViewBag.StatusList = selectList;
        }

        public async Task SetViewBagLocation()
        {
            ViewBag.LocationList = await OptionsAsync("locations");
            await Task.CompletedTask;
        }

        public async Task SetViewBagEmployee()
        {
            ViewBag.EmployeeList = await OptionsAsync("employees");
            await Task.CompletedTask;
        }
    }
}
