using Auto_Garage.Data.AutoGarageDb;
using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Repositories.AdminBookingRepository
{
    public class AdminBookingRepository(AutoGarageDbContext db) : IAdminBookingRepository
    {
        private readonly AutoGarageDbContext _db = db;
        private IQueryable<ServiceBooking> WithIncludes() =>
            _db.ServiceBookings
                .Include(b => b.Vehicle)
                .Include(b => b.ServiceType);

        private IQueryable<ServiceBooking> WithFullIncludes() =>
            _db.ServiceBookings
                .Include(b => b.Vehicle)
                .Include(b => b.ServiceType)
                .Include(b => b.StatusHistories.OrderBy(h => h.ChangedAt));

        public async Task<List<ServiceBooking>> GetAllAsync() =>
            await WithIncludes()
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<List<ServiceBooking>> GetByStatusAsync(BookingStatus status) =>
            await WithIncludes()
                .Where(b => b.Status == status)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<ServiceBooking?> GetByIdAsync(Guid id) =>
            await WithFullIncludes()
                .FirstOrDefaultAsync(b => b.Id == id);

        public async Task<List<ServiceBooking>> GetByMechanicAsync(string mechanicId) =>
            await WithIncludes()
                .Where(b => b.AssignedMechanicId == mechanicId)
                .OrderByDescending(b => b.UpdatedAt)
                .ToListAsync();

        public async Task<ServiceBooking?> GetByIdForMechanicAsync(Guid id, string mechanicId) =>
            await WithFullIncludes()
                .FirstOrDefaultAsync(b => b.Id == id && b.AssignedMechanicId == mechanicId);

        public async Task AddStatusHistoryAsync(BookingStatusHistory history) =>
            await _db.BookingStatusHistories.AddAsync(history);

        public async Task SaveChangesAsync() =>
            await _db.SaveChangesAsync();
    }
}