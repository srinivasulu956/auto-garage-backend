using Auto_Garage.Data.AutoGarageDb;
using Auto_Garage.Models.DomainModels.InvoiceModel;
using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using Auto_Garage.Models.DtoModels.InvoiceDtos;
using Auto_Garage.Repositories.InvoiceRepository;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Services.InvoiceService
{
    public class InvoiceService(
        IInvoiceRepository invoiceRepository,
        AutoGarageDbContext db,
        ILogger<InvoiceService> logger) : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository = invoiceRepository;
        private readonly AutoGarageDbContext _db = db;
        private readonly ILogger<InvoiceService> _logger = logger;
        public async Task<List<InvoiceResponseDto>> GetAllForCustomerAsync(string customerId)
        {
            var invoices = await _invoiceRepository.GetAllByCustomerAsync(customerId);
            return invoices.Select(ToDto).ToList();
        }

        public async Task<InvoiceResponseDto> GetByIdForCustomerAsync(Guid id, string customerId)
        {
            var invoice = await _invoiceRepository.GetByIdForCustomerAsync(id, customerId)
                ?? throw new KeyNotFoundException("Invoice not found.");
            return ToDto(invoice);
        }

        public async Task<InvoiceResponseDto> GetByBookingIdForCustomerAsync(Guid bookingId, string customerId)
        {
            var invoice = await _invoiceRepository.GetByBookingIdForCustomerAsync(bookingId, customerId)
                ?? throw new KeyNotFoundException("Invoice not found for this booking.");
            return ToDto(invoice);
        }

        public async Task<PayInvoiceResponseDto> PayAsync(Guid invoiceId, string customerId, PayInvoiceRequestDto dto)
        {
            var invoice = await _invoiceRepository.GetByIdForCustomerAsync(invoiceId, customerId)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (invoice.Status == InvoiceStatus.Paid)
                throw new InvalidOperationException("This invoice has already been paid.");

            var paymentReference = GenerateFakeTransactionId(dto.PaymentMethod);

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaymentMethod = dto.PaymentMethod;
            invoice.PaymentReference = paymentReference;
            invoice.PaidAt = DateTime.UtcNow;

            var booking = invoice.ServiceBooking;
            booking.Status = BookingStatus.Paid;
            booking.UpdatedAt = DateTime.UtcNow;

            await _db.BookingStatusHistories.AddAsync(new BookingStatusHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.Paid,
                ChangedByUserId = customerId,
                ChangedByRole = "Customer",
                Notes = $"Payment received via {dto.PaymentMethod}. Ref: {paymentReference}"
            });

            await _invoiceRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Invoice {InvoiceId} paid by Customer {CustomerId} via {Method}. Ref: {Ref}",
                invoiceId, customerId, dto.PaymentMethod, paymentReference);

            return new PayInvoiceResponseDto
            {
                Message = "Payment successful.",
                PaymentReference = paymentReference,
                PaidAt = invoice.PaidAt!.Value,
                Invoice = ToDto(invoice)
            };
        }

        public async Task<List<InvoiceResponseDto>> GetAllAsync()
        {
            var invoices = await _invoiceRepository.GetAllAsync();
            return invoices.Select(ToDto).ToList();
        }

        public async Task<InvoiceResponseDto> GenerateAsync(string adminId, GenerateInvoiceRequestDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new ArgumentException("At least one invoice item is required.");

            var booking = await _db.ServiceBookings
                .Include(b => b.ServiceType)
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b => b.Id == dto.BookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.Status != BookingStatus.Completed)
                throw new InvalidOperationException("Invoice can only be generated for completed bookings.");

            if (await _invoiceRepository.ExistsForBookingAsync(dto.BookingId))
                throw new InvalidOperationException("An invoice already exists for this booking.");

            var taxRate = dto.TaxRate ?? 18m;
            var subTotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
            var taxAmount = Math.Round(subTotal * taxRate / 100, 2);
            var totalAmount = subTotal + taxAmount;

            var invoice = new Invoice
            {
                BookingId = dto.BookingId,
                CustomerId = booking.CustomerId,
                SubTotal = subTotal,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                Status = InvoiceStatus.Unpaid,
                Items = dto.Items.Select(i => new InvoiceItem
                {
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _invoiceRepository.AddAsync(invoice);

            booking.Status = BookingStatus.InvoiceGenerated;
            booking.UpdatedAt = DateTime.UtcNow;

            await _db.BookingStatusHistories.AddAsync(new BookingStatusHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.InvoiceGenerated,
                ChangedByUserId = adminId,
                ChangedByRole = "Admin",
                Notes = $"Invoice generated. Total: ₹{totalAmount}"
            });

            await _invoiceRepository.SaveChangesAsync();

            invoice.ServiceBooking = booking;

            _logger.LogInformation(
                "Invoice {InvoiceId} generated by Admin {AdminId} for Booking {BookingId}. Total: {Total}",
                invoice.Id, adminId, dto.BookingId, totalAmount);

            return ToDto(invoice);
        }
        public async Task<InvoiceResponseDto> GetByBookingIdAdminAsync(Guid bookingId)
        {
            var invoice = await _invoiceRepository.GetByBookingIdAdminAsync(bookingId)
                ?? throw new KeyNotFoundException("Invoice not found for this booking.");
            return ToDto(invoice);
        }
        private static InvoiceResponseDto ToDto(Invoice inv) => new()
        {
            Id = inv.Id,
            BookingId = inv.BookingId,
            CustomerId = inv.CustomerId,
            SubTotal = inv.SubTotal,
            TaxRate = inv.TaxRate,
            TaxAmount = inv.TaxAmount,
            TotalAmount = inv.TotalAmount,
            Status = inv.Status,
            StatusLabel = inv.Status == InvoiceStatus.Paid ? "Paid" : "Unpaid",
            PaymentMethod = inv.PaymentMethod,
            PaymentReference = inv.PaymentReference,
            IssuedAt = inv.IssuedAt,
            PaidAt = inv.PaidAt,
            Items = inv.Items.Select(item => new InvoiceItemResponseDto
            {
                Id = item.Id,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Total = item.Quantity * item.UnitPrice
            }).ToList(),
            Booking = new InvoiceBookingSummaryDto
            {
                Id = inv.ServiceBooking.Id,
                ServiceName = inv.ServiceBooking.ServiceType.Name,
                VehicleMake = inv.ServiceBooking.Vehicle.Make,
                VehicleModel = inv.ServiceBooking.Vehicle.Model,
                LicensePlate = inv.ServiceBooking.Vehicle.LicensePlate,
                ScheduledDate = inv.ServiceBooking.ScheduledDate
            }
        };

        private static string GenerateFakeTransactionId(string method)
        {
            var prefix = method?.ToUpper() switch
            {
                "CARD" => "CRD",
                "UPI" => "UPI",
                "CASH" => "CSH",
                _ => "TXN"
            };
            return $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}