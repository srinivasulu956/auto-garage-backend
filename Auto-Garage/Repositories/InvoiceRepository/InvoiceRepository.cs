using Auto_Garage.Data.AutoGarageDb;
using Auto_Garage.Models.DomainModels.InvoiceModel;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Repositories.InvoiceRepository
{
    public class InvoiceRepository(AutoGarageDbContext db) : IInvoiceRepository
    {
        private readonly AutoGarageDbContext _db = db;
        private IQueryable<Invoice> WithIncludes() =>
            _db.Invoices
                .Include(i => i.Items)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(b => b.ServiceType)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(b => b.Vehicle);

        public async Task<List<Invoice>> GetAllByCustomerAsync(string customerId) =>
            await WithIncludes()
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.IssuedAt)
                .ToListAsync();

        public async Task<Invoice?> GetByIdAsync(Guid id) =>
            await WithIncludes()
                .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<Invoice?> GetByIdForCustomerAsync(Guid id, string customerId) =>
            await WithIncludes()
                .FirstOrDefaultAsync(i => i.Id == id && i.CustomerId == customerId);

        public async Task<Invoice?> GetByBookingIdForCustomerAsync(Guid bookingId, string customerId) =>
            await WithIncludes()
                .FirstOrDefaultAsync(i => i.BookingId == bookingId && i.CustomerId == customerId);

        public async Task<List<Invoice>> GetAllAsync() =>
            await WithIncludes()
                .OrderByDescending(i => i.IssuedAt)
                .ToListAsync();
        public async Task<Invoice?> GetByBookingIdAdminAsync(Guid bookingId) =>
            await WithIncludes()
                .FirstOrDefaultAsync(i => i.BookingId == bookingId);

        public async Task<bool> ExistsForBookingAsync(Guid bookingId) =>
            await _db.Invoices.AnyAsync(i => i.BookingId == bookingId);

        public async Task AddAsync(Invoice invoice) =>
            await _db.Invoices.AddAsync(invoice);

        public async Task SaveChangesAsync() =>
            await _db.SaveChangesAsync();
    }
}