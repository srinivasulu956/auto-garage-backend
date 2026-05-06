using Auto_Garage.Data.AutoGarageDb;
using Auto_Garage.Models.DomainModels.JobWorkLogModel;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Repositories.JobWorkLogRepository
{
    public class JobWorkLogRepository(AutoGarageDbContext db) : IJobWorkLogRepository
    {
        private readonly AutoGarageDbContext _db = db;

        public async Task<List<JobWorkLog>> GetByBookingIdAsync(Guid bookingId) =>
            await _db.JobWorkLogs
                .Where(w => w.BookingId == bookingId)
                .OrderBy(w => w.CreatedAt)
                .ToListAsync();

        public async Task AddAsync(JobWorkLog item) =>
            await _db.JobWorkLogs.AddAsync(item);

        public async Task<JobWorkLog?> GetByIdAsync(Guid id) =>
            await _db.JobWorkLogs.FirstOrDefaultAsync(w => w.Id == id);

        public async Task DeleteAsync(Guid id)
        {
            var item = await _db.JobWorkLogs.FindAsync(id);
            if (item != null) _db.JobWorkLogs.Remove(item);
        }

        public async Task SaveChangesAsync() =>
            await _db.SaveChangesAsync();
    }
}