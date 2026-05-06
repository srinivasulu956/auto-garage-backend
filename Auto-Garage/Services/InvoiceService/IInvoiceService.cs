using Auto_Garage.Models.DtoModels.InvoiceDtos;

namespace Auto_Garage.Services.InvoiceService
{
    public interface IInvoiceService
    {
        // Customer operations 
        Task<List<InvoiceResponseDto>> GetAllForCustomerAsync(string customerId);
        Task<InvoiceResponseDto> GetByIdForCustomerAsync(Guid id, string customerId);
        Task<InvoiceResponseDto> GetByBookingIdForCustomerAsync(Guid bookingId, string customerId);
        Task<PayInvoiceResponseDto> PayAsync(Guid invoiceId, string customerId, PayInvoiceRequestDto dto);

        // Admin operations 
        Task<List<InvoiceResponseDto>> GetAllAsync();
        Task<InvoiceResponseDto> GenerateAsync(string adminId, GenerateInvoiceRequestDto dto);
        Task<InvoiceResponseDto> GetByBookingIdAdminAsync(Guid bookingId);  // no ownership check
    }
}