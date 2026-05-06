using Auto_Garage.Models.DomainModels.ServiceBookingModel;

namespace Auto_Garage.Repositories.AdminBookingRepository
{
    public interface IAdminBookingRepository
    {
        Task<List<ServiceBooking>> GetAllAsync();
        Task<List<ServiceBooking>> GetByStatusAsync(BookingStatus status);
        Task<ServiceBooking?> GetByIdAsync(Guid id);
        Task<List<ServiceBooking>> GetByMechanicAsync(string mechanicId);
        Task<ServiceBooking?> GetByIdForMechanicAsync(Guid id, string mechanicId);
        Task AddStatusHistoryAsync(BookingStatusHistory history);
        Task SaveChangesAsync();
    }
}