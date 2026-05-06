using Auto_Garage.Models.DtoModels.AdminBookingDtos;
using Auto_Garage.Services.AdminBookingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auto_Garage.Controllers
{
    [ApiController]
    [Route("api/admin/bookings")]
    [Authorize(Roles = "Admin")]
    public class AdminBookingController(
        IAdminBookingService adminBookingService,
        ILogger<AdminBookingController> logger) : ControllerBase
    {
        private readonly IAdminBookingService _service = adminBookingService;
        private readonly ILogger<AdminBookingController> _logger = logger;

        private string GetAdminId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ── GET /api/admin/bookings ───────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // ── GET /api/admin/bookings/{id} ──────────────────────────────────────

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── GET /api/admin/bookings/mechanics ─────────────────────────────────

        [HttpGet("mechanics")]
        public async Task<IActionResult> GetMechanics()
        {
            var result = await _service.GetMechanicsAsync();
            return Ok(result);
        }

        // ── PATCH /api/admin/bookings/{id}/confirm ────────────────────────────
        // Pending → Confirmed

        [HttpPatch("{id:guid}/confirm")]
        public async Task<IActionResult> Confirm(Guid id, [FromBody] ConfirmBookingRequestDto dto)
        {
            try
            {
                var result = await _service.ConfirmAsync(id, GetAdminId(), dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── PATCH /api/admin/bookings/{id}/assign ─────────────────────────────
        // Confirmed → AssignedToMechanic

        [HttpPatch("{id:guid}/assign")]
        public async Task<IActionResult> AssignMechanic(Guid id, [FromBody] AssignMechanicRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.AssignMechanicAsync(id, GetAdminId(), dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── PATCH /api/admin/bookings/{id}/reassign ───────────────────────────
        // QualityCheck → AssignedToMechanic (send back for rework)

        [HttpPatch("{id:guid}/reassign")]
        public async Task<IActionResult> ReassignMechanic(Guid id, [FromBody] ReassignMechanicRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.ReassignMechanicAsync(id, GetAdminId(), dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── PATCH /api/admin/bookings/{id}/status ─────────────────────────────
        // QualityCheck → Completed

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBookingStatusRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.UpdateStatusAsync(id, GetAdminId(), dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}