using Auto_Garage.Models.DtoModels.AdminBookingDtos;

namespace Auto_Garage.Services.AdminBookingService
{
    public interface IAdminBookingService
    {
        // Admin
        Task<List<AdminBookingResponseDto>> GetAllAsync();
        Task<AdminBookingDetailResponseDto> GetByIdAsync(Guid id);
        Task<AdminBookingResponseDto> ConfirmAsync(Guid id, string adminId, ConfirmBookingRequestDto dto);
        Task<AdminBookingResponseDto> AssignMechanicAsync(Guid id, string adminId, AssignMechanicRequestDto dto);
        Task<AdminBookingResponseDto> ReassignMechanicAsync(Guid id, string adminId, ReassignMechanicRequestDto dto);
        Task<AdminBookingResponseDto> UpdateStatusAsync(Guid id, string adminId, UpdateBookingStatusRequestDto dto);
        Task<List<MechanicStaffDto>> GetMechanicsAsync();

        // Mechanic
        Task<List<AdminBookingResponseDto>> GetMyJobsAsync(string mechanicId);
        Task<AdminBookingDetailResponseDto> GetMyJobByIdAsync(Guid id, string mechanicId);
        Task<AdminBookingResponseDto> UpdateJobStatusAsync(Guid id, string mechanicId, MechanicUpdateStatusRequestDto dto);
    }
}