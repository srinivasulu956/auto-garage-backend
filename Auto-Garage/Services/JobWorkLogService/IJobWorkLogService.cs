using Auto_Garage.Models.DtoModels.JobWorkLogDtos;
using Auto_Garage.Models.DomainModels.JobWorkLogModel;
using Auto_Garage.Repositories.JobWorkLogRepository;
using Auto_Garage.Repositories.AdminBookingRepository;

namespace Auto_Garage.Services.JobWorkLogService
{
    public interface IJobWorkLogService
    {
        // Mechanic
        Task<List<WorkLogItemResponseDto>> GetByBookingIdAsync(Guid bookingId);
        Task<WorkLogItemResponseDto> AddAsync(Guid bookingId, string mechanicId, AddWorkLogItemRequestDto dto);
        Task DeleteAsync(Guid itemId, string mechanicId);

        // Admin (same query, no ownership check on mechanic)
        Task<List<WorkLogItemResponseDto>> GetByBookingIdAdminAsync(Guid bookingId);
    }
}