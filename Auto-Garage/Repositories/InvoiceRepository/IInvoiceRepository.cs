using Auto_Garage.Models.DomainModels.InvoiceModel;

namespace Auto_Garage.Repositories.InvoiceRepository
{
    public interface IInvoiceRepository
    {
        Task<List<Invoice>> GetAllByCustomerAsync(string customerId);
        Task<Invoice?> GetByIdAsync(Guid id);
        Task<Invoice?> GetByIdForCustomerAsync(Guid id, string customerId);
        Task<Invoice?> GetByBookingIdForCustomerAsync(Guid bookingId, string customerId);
        Task<List<Invoice>> GetAllAsync();
        Task<Invoice?> GetByBookingIdAdminAsync(Guid bookingId);  // no ownership check
        Task<bool> ExistsForBookingAsync(Guid bookingId);
        Task AddAsync(Invoice invoice);
        Task SaveChangesAsync();
    }
}