using Auto_Garage.Models.DtoModels.JobWorkLogDtos;
using Auto_Garage.Services.JobWorkLogService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auto_Garage.Controllers
{
    [ApiController]
    public class JobWorkLogController(
        IJobWorkLogService service,
        ILogger<JobWorkLogController> logger) : ControllerBase
    {
        private readonly IJobWorkLogService _service = service;
        private readonly ILogger<JobWorkLogController> _logger = logger;

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ════════════════════════════════════════════════════════════════════
        // MECHANIC ENDPOINTS
        // ════════════════════════════════════════════════════════════════════

        // ── GET /api/mechanic/jobs/{bookingId}/worklog ─────────────────────
        [HttpGet("api/mechanic/jobs/{bookingId:guid}/worklog")]
        [Authorize(Roles = "Mechanic")]
        public async Task<IActionResult> GetMyWorkLog(Guid bookingId)
        {
            var result = await _service.GetByBookingIdAsync(bookingId);
            return Ok(result);
        }

        // ── POST /api/mechanic/jobs/{bookingId}/worklog ────────────────────
        [HttpPost("api/mechanic/jobs/{bookingId:guid}/worklog")]
        [Authorize(Roles = "Mechanic")]
        public async Task<IActionResult> AddWorkLogItem(Guid bookingId, [FromBody] AddWorkLogItemRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.AddAsync(bookingId, GetUserId(), dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── DELETE /api/mechanic/jobs/worklog/{itemId} ─────────────────────
        [HttpDelete("api/mechanic/jobs/worklog/{itemId:guid}")]
        [Authorize(Roles = "Mechanic")]
        public async Task<IActionResult> DeleteWorkLogItem(Guid itemId)
        {
            try
            {
                await _service.DeleteAsync(itemId, GetUserId());
                return Ok(new { message = "Item removed." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(); }
        }

        // ════════════════════════════════════════════════════════════════════
        // ADMIN ENDPOINTS
        // ════════════════════════════════════════════════════════════════════

        // ── GET /api/admin/bookings/{bookingId}/worklog ────────────────────
        [HttpGet("api/admin/bookings/{bookingId:guid}/worklog")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWorkLogAdmin(Guid bookingId)
        {
            var result = await _service.GetByBookingIdAdminAsync(bookingId);
            return Ok(result);
        }
    }
}