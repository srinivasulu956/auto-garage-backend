using Auto_Garage.Models.DomainModels.VehicleModel;

namespace Auto_Garage.Repositories.VehicleRepository
{
    public interface IVehicleRepository
    {
        Task<List<Vehicle>> GetActiveByCustomerAsync(string customerId);
        Task<List<Vehicle>> GetInactiveByCustomerAsync(string customerId);
        Task<Vehicle?> GetByIdForCustomerAsync(Guid id, string customerId);
        Task<bool> PlateExistsForCustomerAsync(string plate, string customerId, Guid? excludeId = null);
        Task<bool> HasActiveBookingAsync(Guid vehicleId);
        Task<bool> HasAnyBookingAsync(Guid vehicleId);
        Task AddAsync(Vehicle vehicle);
        Task SaveChangesAsync();
        Task<List<Vehicle>> GetByCustomerIdAsync(string customerId);
    }
}