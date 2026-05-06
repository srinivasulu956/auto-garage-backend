using Auto_Garage.Data.AutoGarageDb;
using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using Auto_Garage.Models.DomainModels.VehicleModel;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Repositories.VehicleRepository
{
    public class VehicleRepository(AutoGarageDbContext db) : IVehicleRepository
    {
        private readonly AutoGarageDbContext _db = db;

        public async Task<List<Vehicle>> GetActiveByCustomerAsync(string customerId) =>
            await _db.Vehicles
                .Where(v => v.CustomerId == customerId && v.IsActive)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

        public async Task<List<Vehicle>> GetInactiveByCustomerAsync(string customerId) =>
            await _db.Vehicles
                .Where(v => v.CustomerId == customerId && !v.IsActive)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

        public async Task<Vehicle?> GetByIdForCustomerAsync(Guid id, string customerId) =>
            await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id && v.CustomerId == customerId);

        public async Task<bool> PlateExistsForCustomerAsync(string plate, string customerId, Guid? excludeId = null) =>
            await _db.Vehicles.AnyAsync(v =>
                v.LicensePlate.ToLower() == plate.ToLower() &&
                v.CustomerId == customerId &&
                (excludeId == null || v.Id != excludeId));

        public async Task<bool> HasActiveBookingAsync(Guid vehicleId) =>
            await _db.ServiceBookings.AnyAsync(b =>
                b.VehicleId == vehicleId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Paid);

        public async Task<bool> HasAnyBookingAsync(Guid vehicleId) =>
            await _db.ServiceBookings.AnyAsync(b => b.VehicleId == vehicleId);

        public async Task AddAsync(Vehicle vehicle) =>
            await _db.Vehicles.AddAsync(vehicle);

        public async Task SaveChangesAsync() =>
            await _db.SaveChangesAsync();

        public async Task<List<Vehicle>> GetByCustomerIdAsync(string customerId)
        {
            return await _db.Vehicles
                .Where(v => v.CustomerId == customerId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }
    }
}