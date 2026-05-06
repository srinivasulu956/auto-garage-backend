using Auto_Garage.Services.AdminUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auto_Garage.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminUserController(
        IAdminUserService adminUserService,
        ILogger<AdminUserController> logger) : ControllerBase
    {
        private readonly IAdminUserService _service = adminUserService;
        private readonly ILogger<AdminUserController> _logger = logger;

        // ════════════════════════════════════════════════════════════════════
        // CUSTOMERS
        // ════════════════════════════════════════════════════════════════════

        // ── GET /api/admin/customers ──────────────────────────────────────────
        [HttpGet("customers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var result = await _service.GetAllCustomersAsync();
            return Ok(result);
        }

        // ── GET /api/admin/customers/{id} ─────────────────────────────────────
        [HttpGet("customers/{id}")]
        public async Task<IActionResult> GetCustomerById(string id)
        {
            try
            {
                var result = await _service.GetCustomerByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── PATCH /api/admin/customers/{id}/toggle-active ─────────────────────
        [HttpPatch("customers/{id}/toggle-active")]
        public async Task<IActionResult> ToggleCustomerActive(string id)
        {
            try
            {
                var result = await _service.ToggleCustomerActiveAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // STAFF
        // ════════════════════════════════════════════════════════════════════

        // ── GET /api/admin/staff ──────────────────────────────────────────────
        [HttpGet("staff")]
        public async Task<IActionResult> GetAllStaff()
        {
            var result = await _service.GetAllStaffAsync();
            return Ok(result);
        }

        // ── PATCH /api/admin/staff/{id}/toggle-active ─────────────────────────
        [HttpPatch("staff/{id}/toggle-active")]
        public async Task<IActionResult> ToggleStaffActive(string id)
        {
            try
            {
                var result = await _service.ToggleStaffActiveAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}