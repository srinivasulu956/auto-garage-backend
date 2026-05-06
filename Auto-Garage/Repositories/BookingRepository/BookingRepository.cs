using Auto_Garage.Data.AutoGarageDb;
using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Repositories.BookingRepository
{
    public class BookingRepository(AutoGarageDbContext db) : IBookingRepository
    {
        private readonly AutoGarageDbContext _db = db;

        public async Task<List<ServiceBooking>> GetAllByCustomerAsync(string customerId) =>
            await _db.ServiceBookings
                .Where(b => b.CustomerId == customerId)
                .Include(b => b.Vehicle)
                .Include(b => b.ServiceType)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<ServiceBooking?> GetByIdForCustomerAsync(Guid id, string customerId) =>
            await _db.ServiceBookings
                .Include(b => b.Vehicle)
                .Include(b => b.ServiceType)
                .Include(b => b.StatusHistories.OrderBy(h => h.ChangedAt))
                .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == customerId);

        public async Task<bool> HasActiveBookingForVehicleAsync(Guid vehicleId) =>
            await _db.ServiceBookings.AnyAsync(b =>
                b.VehicleId == vehicleId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Paid);

        public async Task AddAsync(ServiceBooking booking) =>
            await _db.ServiceBookings.AddAsync(booking);

        public async Task AddStatusHistoryAsync(BookingStatusHistory history) =>
            await _db.BookingStatusHistories.AddAsync(history);

        public async Task SaveChangesAsync() =>
            await _db.SaveChangesAsync();
    }
}