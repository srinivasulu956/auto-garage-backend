using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auto_Garage.Models.DomainModels.ServiceTypeModel
{
    public class ServiceType
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }

        [Required]
        public double EstimatedHours { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property

        public ICollection<ServiceBooking> ServiceBookings { get; set; } = new List<ServiceBooking>(); // show me all bookings for this service type
    }
}
