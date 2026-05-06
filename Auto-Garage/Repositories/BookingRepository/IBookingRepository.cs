using Auto_Garage.Models.DomainModels.ServiceBookingModel;

namespace Auto_Garage.Repositories.BookingRepository
{
    public interface IBookingRepository
    {
        Task<List<ServiceBooking>> GetAllByCustomerAsync(string customerId);
        Task<ServiceBooking?> GetByIdForCustomerAsync(Guid id, string customerId);
        Task<bool> HasActiveBookingForVehicleAsync(Guid vehicleId);
        Task AddAsync(ServiceBooking booking);
        Task AddStatusHistoryAsync(BookingStatusHistory history);
        Task SaveChangesAsync();
    }
}