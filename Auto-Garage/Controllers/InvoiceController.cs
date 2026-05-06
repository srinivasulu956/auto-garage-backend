using Auto_Garage.Models.DtoModels.InvoiceDtos;
using Auto_Garage.Services.InvoiceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auto_Garage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController(
        IInvoiceService invoiceService,
        ILogger<InvoiceController> logger) : ControllerBase
    {
        private readonly IInvoiceService _invoiceService = invoiceService;
        private readonly ILogger<InvoiceController> _logger = logger;

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ════════════════════════════════════════════════════════════════════
        // CUSTOMER ENDPOINTS
        // ════════════════════════════════════════════════════════════════════

        // ── GET /api/invoice ─────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _invoiceService.GetAllForCustomerAsync(GetUserId());
            return Ok(result);
        }

        // ── GET /api/invoice/{id} ────────────────────────────────────────────
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _invoiceService.GetByIdForCustomerAsync(id, GetUserId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── GET /api/invoice/booking/{bookingId} ─────────────────────────────
        [HttpGet("booking/{bookingId:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetByBookingId(Guid bookingId)
        {
            try
            {
                var result = await _invoiceService.GetByBookingIdForCustomerAsync(bookingId, GetUserId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── POST /api/invoice/{id}/pay ────────────────────────────────────────
        [HttpPost("{id:guid}/pay")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Pay(Guid id, [FromBody] PayInvoiceRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _invoiceService.PayAsync(id, GetUserId(), dto);
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

        // ════════════════════════════════════════════════════════════════════
        // ADMIN ENDPOINTS
        // ════════════════════════════════════════════════════════════════════

        // ── POST /api/invoice ─────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Generate([FromBody] GenerateInvoiceRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _invoiceService.GenerateAsync(GetUserId(), dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── GET /api/invoice/admin/all ────────────────────────────────────────
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAdmin()
        {
            var result = await _invoiceService.GetAllAsync();
            return Ok(result);
        }

        // ── GET /api/invoice/admin/booking/{bookingId} ────────────────────────
        // Admin version — no customer ownership check.
        // Used on the booking detail page to show/download invoice for
        // InvoiceGenerated and Paid bookings.
        [HttpGet("admin/booking/{bookingId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByBookingIdAdmin(Guid bookingId)
        {
            try
            {
                var result = await _invoiceService.GetByBookingIdAdminAsync(bookingId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}