using Auto_Garage.Models.DtoModels.AdminBookingDtos;
using Auto_Garage.Services.AdminBookingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auto_Garage.Controllers
{
    [ApiController]
    [Route("api/mechanic/jobs")]
    [Authorize(Roles = "Mechanic")]
    public class MechanicController(
        IAdminBookingService adminBookingService,
        ILogger<MechanicController> logger) : ControllerBase
    {
        private readonly IAdminBookingService _service = adminBookingService;
        private readonly ILogger<MechanicController> _logger = logger;

        private string GetMechanicId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ── GET /api/mechanic/jobs ────────────────────────────────────────────
        // All jobs assigned to this mechanic

        [HttpGet]
        public async Task<IActionResult> GetMyJobs()
        {
            var result = await _service.GetMyJobsAsync(GetMechanicId());
            return Ok(result);
        }

        // ── GET /api/mechanic/jobs/{id} ───────────────────────────────────────

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service.GetMyJobByIdAsync(id, GetMechanicId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── PATCH /api/mechanic/jobs/{id}/status ──────────────────────────────
        // Mechanic moves job through: AssignedToMechanic → InProgress → WaitingForParts / QualityCheck

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] MechanicUpdateStatusRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.UpdateJobStatusAsync(id, GetMechanicId(), dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}