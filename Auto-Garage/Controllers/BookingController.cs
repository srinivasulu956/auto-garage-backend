using Auto_Garage.Models.DtoModels.BookingDtos;
using Auto_Garage.Services.BookingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auto_Garage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class BookingController(
        IBookingService bookingService,
        ILogger<BookingController> logger) : ControllerBase
    {
        private readonly IBookingService _bookingService = bookingService;
        private readonly ILogger<BookingController> _logger = logger;

        private string GetCustomerId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ── GET /api/booking ─────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _bookingService.GetAllAsync(GetCustomerId());
            return Ok(result);
        }

        // ── GET /api/booking/{id} ────────────────────────────────────────────

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _bookingService.GetByIdAsync(id, GetCustomerId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── POST /api/booking ────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _bookingService.CreateAsync(GetCustomerId(), dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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

        // ── PUT /api/booking/{id} ────────────────────────────────────────────

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _bookingService.UpdateAsync(id, GetCustomerId(), dto);
                return Ok(result);
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

        // ── DELETE /api/booking/{id} ─────────────────────────────────────────

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                await _bookingService.CancelAsync(id, GetCustomerId());
                return NoContent();
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
    }
}