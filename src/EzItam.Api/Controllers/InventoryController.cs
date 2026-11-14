namespace EzItam.Api.Controllers
{

    [ApiController]
    [Route("api/v1/inventories")]
    public class InventoryController(IInventoryService inventoryService) : ControllerBase
    {
        private readonly IInventoryService _inventoryService = inventoryService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAllInventories(CancellationToken ct)
        {
            var inventories = await _inventoryService.GetAllInventoriesAsync(ct);
            return Ok(inventories);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<InventoryDto>> GetInventoryById(int id, CancellationToken ct)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id, ct);
            if (inventory is null)
            {
                return InventoryNotFound();
            }
            return Ok(inventory);
        }

        [HttpPost]
        [Authorize(Policy = "CanEdit")]
        public async Task<ActionResult<InventoryDto>> CreateInventory(CreateInventoryDto dto, CancellationToken ct)
        {
            var id = await _inventoryService.CreateInventoryAsync(dto, ct);
            var inventory = await _inventoryService.GetInventoryByIdAsync(id, ct)
                ?? throw new InvalidOperationException("The created inventory could not be loaded.");
            return CreatedAtAction(nameof(GetInventoryById), new { id }, inventory);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "CanEdit")]
        public async Task<ActionResult> UpdateInventory(int id, UpdateInventoryDto dto, CancellationToken ct)
        {
            var updated = await _inventoryService.UpdateInventoryAsync(id, dto, ct);
            if (!updated)
            {
                return InventoryNotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteInventory(int id, CancellationToken ct)
        {
            var deleted = await _inventoryService.DeleteInventoryAsync(id, ct);
            if (!deleted)
            {
                return InventoryNotFound();
            }
            return NoContent();
        }

        [HttpPut("{id:int}/close")]
        [Authorize(Policy = "CanEdit")]
        public async Task<ActionResult> CloseInventory(int id, CancellationToken ct)
        {
            var closed = await _inventoryService.CloseInventoryAsync(id, ct);
            if (!closed)
            {
                return InventoryNotFound();
            }
            return NoContent();
        }

        [HttpPost("{id:int}/genbook")]
        [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> GenInventoryBook(int id, CancellationToken ct)
        {
            await _inventoryService.GenInventoryBookAsync(id, ct);
            return NoContent();
        }

        [Authorize(Policy = "CanEdit")]
        [HttpGet("{id:int}/details")]
        public async Task<ActionResult<InventoryDetailResponseDto>> GetInventoryDetailByInventoryId(int id, CancellationToken ct)
        {
            var res = await _inventoryService.GetInventoryDetailByInventoryIdAsync(id, ct);
            return res is null ? InventoryNotFound() : Ok(res);
        }

        [Authorize(Policy = "CanEdit")]
        [HttpGet("{id:int}/asset")]
        public async Task<ActionResult<InventoryDetailDto?>> GetInventoryDetailByInventoryId([FromQuery] string tag, int id, CancellationToken ct)
        {
            var res = await _inventoryService.GetDetailByTag(id, tag, ct);
            return Ok(res);
        }

        [Authorize(Policy = "CanEdit")]
        [HttpPost("scan")]
        public async Task<ActionResult<bool>> ScanResult([FromBody] ScanResultRequest req, CancellationToken ct)
        {
            var res = await _inventoryService.PostScanResult(req, ct);
            return Ok(res);
        }

        [Authorize(Policy = "CanEdit")]
        [HttpPost("postdiff/{id:int}")]
        public async Task<IActionResult> PostDiff(int id, CancellationToken ct)
        {
            await _inventoryService.PostDiff(id, ct);
            return NoContent();
        }

        private NotFoundObjectResult InventoryNotFound()
            => NotFound(new ApiError { Message = "인벤토리를 찾을 수 없습니다." });
    }
}
