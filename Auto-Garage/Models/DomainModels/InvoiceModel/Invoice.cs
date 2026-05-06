using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auto_Garage.Models.DomainModels.InvoiceModel
{
    public enum InvoiceStatus
    {
        Unpaid = 0,
        Paid = 1
    }

    public class Invoice
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }      

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxRate { get; set; } = 18; 

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }    // SubTotal + TaxAmount

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;

        [MaxLength(50)]
        public string? PaymentMethod { get; set; } 

        [MaxLength(100)]
        public string? PaymentReference { get; set; } 

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        // Navigation
        [ForeignKey("BookingId")]
        public ServiceBooking ServiceBooking { get; set; } = null!;

        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InvoiceId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty; 

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total => Quantity * UnitPrice; 

        // Navigation
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; } = null!;
    }
}