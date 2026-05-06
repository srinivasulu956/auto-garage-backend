using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using System.ComponentModel.DataAnnotations;

namespace Auto_Garage.Models.DomainModels.VehicleModel
{
    public class Vehicle
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Range(1980, 2100)]
        public int Year { get; set; }

        [Required]
        [MaxLength(50)]
        public string LicensePlate { get; set; } = string.Empty;

        [MaxLength(17)]
        public string? VIN { get; set; }

        [Required]
        [MaxLength(20)]
        public string FuelType { get; set; } = "Petrol";

        [MaxLength(50)]
        public string? Nickname { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ServiceBooking> ServiceBookings { get; set; } = new List<ServiceBooking>();
    }
}