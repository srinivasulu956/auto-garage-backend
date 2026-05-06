using Auto_Garage.Models.DomainModels.ServiceTypeModel;
using Auto_Garage.Models.DomainModels.VehicleModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auto_Garage.Models.DomainModels.ServiceBookingModel
{
    public class ServiceBooking
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public Guid ServiceTypeId { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required]
        public DateTime ScheduledDate { get; set; }

        [MaxLength(1000)]
        public string? CustomerNotes { get; set; } = string.Empty;

        public string? AssignedMechanicId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BookedBasePrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Vehicle Vehicle { get; set; } = null!;
        public ServiceType ServiceType { get; set; } = null!;
        public ICollection<BookingStatusHistory> StatusHistories { get; set; } = new List<BookingStatusHistory>();
    }
}