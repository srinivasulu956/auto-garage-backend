using Auto_Garage.Models.DomainModels.JobWorkLogModel;
using Auto_Garage.Models.DtoModels.JobWorkLogDtos;
using Auto_Garage.Repositories.AdminBookingRepository;
using Auto_Garage.Repositories.JobWorkLogRepository;

namespace Auto_Garage.Services.JobWorkLogService
{
    public class JobWorkLogService(
         IJobWorkLogRepository repo,
         IAdminBookingRepository bookingRepo,
         ILogger<JobWorkLogService> logger) : IJobWorkLogService
    {
        private readonly IJobWorkLogRepository _repo = repo;
        private readonly IAdminBookingRepository _bookingRepo = bookingRepo;
        private readonly ILogger<JobWorkLogService> _logger = logger;

        public async Task<List<WorkLogItemResponseDto>> GetByBookingIdAsync(Guid bookingId)
        {
            var items = await _repo.GetByBookingIdAsync(bookingId);
            return items.Select(ToDto).ToList();
        }

        public async Task<WorkLogItemResponseDto> AddAsync(Guid bookingId, string mechanicId, AddWorkLogItemRequestDto dto)
        {
            // Make sure the booking exists and belongs to this mechanic
            var booking = await _bookingRepo.GetByIdForMechanicAsync(bookingId, mechanicId)
                ?? throw new KeyNotFoundException("Job not found or not assigned to you.");

            // Only allow adding work log while job is active (not yet at QC or beyond)
            var finalStates = new[]
            {
                Models.DomainModels.ServiceBookingModel.BookingStatus.QualityCheck,
                Models.DomainModels.ServiceBookingModel.BookingStatus.Completed,
                Models.DomainModels.ServiceBookingModel.BookingStatus.InvoiceGenerated,
                Models.DomainModels.ServiceBookingModel.BookingStatus.Paid,
                Models.DomainModels.ServiceBookingModel.BookingStatus.Cancelled,
            };

            if (finalStates.Contains(booking.Status))
                throw new InvalidOperationException("Work log items cannot be added once the job is in Quality Check or beyond.");

            var item = new JobWorkLog
            {
                BookingId = bookingId,
                MechanicId = mechanicId,
                Description = dto.Description,
                Quantity = dto.Quantity,
                UnitCost = dto.UnitCost,
            };

            await _repo.AddAsync(item);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("WorkLog item {Id} added to Booking {BookingId} by Mechanic {MechanicId}", item.Id, bookingId, mechanicId);

            return ToDto(item);
        }

        public async Task DeleteAsync(Guid itemId, string mechanicId)
        {
            var item = await _repo.GetByIdAsync(itemId)
                ?? throw new KeyNotFoundException("Work log item not found.");

            if (item.MechanicId != mechanicId)
                throw new UnauthorizedAccessException("You can only delete your own work log items.");

            await _repo.DeleteAsync(itemId);
            await _repo.SaveChangesAsync();
        }

        public async Task<List<WorkLogItemResponseDto>> GetByBookingIdAdminAsync(Guid bookingId)
        {
            var items = await _repo.GetByBookingIdAsync(bookingId);
            return items.Select(ToDto).ToList();
        }

        private static WorkLogItemResponseDto ToDto(JobWorkLog w) => new()
        {
            Id = w.Id,
            BookingId = w.BookingId,
            MechanicId = w.MechanicId,
            Description = w.Description,
            Quantity = w.Quantity,
            UnitCost = w.UnitCost,
            LineTotal = w.Quantity * w.UnitCost,
            CreatedAt = w.CreatedAt,
        };
    }
}
