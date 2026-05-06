using Auto_Garage.Models.DomainModels.InvoiceModel;
using System.ComponentModel.DataAnnotations;

namespace Auto_Garage.Models.DtoModels.InvoiceDtos
{
    public class InvoiceItemRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Range(0, 999999)]
        public decimal UnitPrice { get; set; }
    }

    public class GenerateInvoiceRequestDto
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public List<InvoiceItemRequestDto> Items { get; set; } = new();

        [Range(0, 100)]
        public decimal? TaxRate { get; set; }
    }

    public class PayInvoiceRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
        public string? CardLastFour { get; set; }
        public string? UpiId { get; set; }
    }

    public class PayInvoiceResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        public InvoiceResponseDto Invoice { get; set; } = null!;
    }

    public class InvoiceItemResponseDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }

    public class InvoiceResponseDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public List<InvoiceItemResponseDto> Items { get; set; } = new();
        public InvoiceBookingSummaryDto Booking { get; set; } = null!;
    }

    public class InvoiceBookingSummaryDto
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
    }
}