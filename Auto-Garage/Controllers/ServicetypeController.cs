using Auto_Garage.Models.DtoModels.ServiceTypeDtos;
using Auto_Garage.Services.ServiceTypeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auto_Garage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypeController(
        IServiceTypeService serviceTypeService,
        ILogger<ServiceTypeController> logger) : ControllerBase
    {
        private readonly IServiceTypeService _serviceTypeService = serviceTypeService;
        private readonly ILogger<ServiceTypeController> _logger = logger;

        // ── GET /api/servicetype ─────────────────────────────────────────────

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _serviceTypeService.GetAllAsync();
            return Ok(result);
        }

        // ── POST /api/servicetype ────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AddServiceTypeRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _serviceTypeService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetAll), result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── PUT /api/servicetype/{id} ────────────────────────────────────────

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AddServiceTypeRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _serviceTypeService.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── DELETE /api/servicetype/{id} ─────────────────────────────────────

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _serviceTypeService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("inactive")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetInactive()
        {
            var result = await _serviceTypeService.GetInactiveAsync();
            return Ok(result);
        }

        [HttpPut("{id:guid}/reactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reactivate(Guid id)
        {
            try
            {
                await _serviceTypeService.ReactivateAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}