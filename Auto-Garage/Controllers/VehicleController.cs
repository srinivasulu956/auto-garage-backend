using Auto_Garage.Models.DtoModels.VehicleDtos;
using Auto_Garage.Services.VehicleService;
using Auto_Garage.Repositories.BookingRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auto_Garage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer,Admin")]
    public class VehicleController(
        IVehicleService vehicleService,
        IBookingRepository bookingRepository,
        ILogger<VehicleController> logger) : ControllerBase
    {
        private readonly IVehicleService _vehicleService = vehicleService;
        private readonly IBookingRepository _bookingRepository = bookingRepository;
        private readonly ILogger<VehicleController> _logger = logger;

        private string GetCustomerId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ── GET /api/vehicle ─────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _vehicleService.GetAllActiveAsync(GetCustomerId());
            return Ok(result);
        }

        // ── GET /api/vehicle/inactive ────────────────────────────────────────

        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive()
        {
            var result = await _vehicleService.GetAllInactiveAsync(GetCustomerId());
            return Ok(result);
        }

        // ── GET /api/vehicle/busy-ids ────────────────────────────────────────
        // Returns vehicle IDs that currently have an active (non-Paid, non-Cancelled)
        // booking. Used by the new-booking wizard to disable those vehicles in the UI.

        [HttpGet("busy-ids")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetBusyVehicleIds()
        {
            var vehicles = await _vehicleService.GetAllActiveAsync(GetCustomerId());
            var busyIds = new List<Guid>();

            foreach (var v in vehicles)
            {
                if (await _bookingRepository.HasActiveBookingForVehicleAsync(v.Id))
                    busyIds.Add(v.Id);
            }

            return Ok(busyIds);
        }

        // ── GET /api/vehicle/{id} ────────────────────────────────────────────

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _vehicleService.GetByIdAsync(id, GetCustomerId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── POST /api/vehicle ────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddVehicleRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _vehicleService.CreateAsync(GetCustomerId(), dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── PUT /api/vehicle/{id} ────────────────────────────────────────────

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _vehicleService.UpdateAsync(id, GetCustomerId(), dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── PATCH /api/vehicle/{id}/reactivate ───────────────────────────────

        [HttpPatch("{id:guid}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id)
        {
            try
            {
                var result = await _vehicleService.ReactivateAsync(id, GetCustomerId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── DELETE /api/vehicle/{id} ─────────────────────────────────────────

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _vehicleService.DeleteAsync(id, GetCustomerId());
                return Ok(new { message = "Vehicle moved to inactive." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── GET /api/vehicle/admin/customer/{customerId} ─────────────────────

        [HttpGet("admin/customer/{customerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetVehiclesByCustomer(string customerId)
        {
            try
            {
                var result = await _vehicleService.GetAllByCustomerIdAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicles for admin");
                return BadRequest(new { message = "Failed to fetch vehicles" });
            }
        }
    }
}