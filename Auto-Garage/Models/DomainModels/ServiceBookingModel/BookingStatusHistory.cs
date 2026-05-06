using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auto_Garage.Models.DomainModels.ServiceBookingModel
{
    public class BookingStatusHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public BookingStatus Status { get; set; }

        [Required]
        public string ChangedByUserId { get; set; } = string.Empty; 

        public string ChangedByRole { get; set; } = string.Empty;   

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("BookingId")]
        public ServiceBooking ServiceBooking { get; set; } = null!;
    }
}
