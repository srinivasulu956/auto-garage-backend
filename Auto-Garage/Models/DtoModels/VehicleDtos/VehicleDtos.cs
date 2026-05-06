
using System.ComponentModel.DataAnnotations;

namespace Auto_Garage.Models.DtoModels.VehicleDtos
{
    public class AddVehicleRequestDto
    {
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
        [MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [MaxLength(17)]
        public string? VIN { get; set; }

        [Required]
        [MaxLength(20)]
        public string FuelType { get; set; } = "Petrol";

        [MaxLength(50)]
        public string? Nickname { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    public class UpdateVehicleRequestDto
    {
        // Core fields — only editable when no booking history exists
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? LicensePlate { get; set; }
        public string? VIN { get; set; }
        public string? FuelType { get; set; }

        // Always editable
        public string? Nickname { get; set; }
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }
    }

    public class VehicleResponseDto
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string? VIN { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string? Nickname { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public bool HasBookingHistory { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}