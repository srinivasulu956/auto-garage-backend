using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using Auto_Garage.Models.DtoModels.ServiceTypeDtos;
using Auto_Garage.Models.DtoModels.VehicleDtos;
using System.ComponentModel.DataAnnotations;

namespace Auto_Garage.Models.DtoModels.BookingDtos
{
    public class CreateBookingRequestDto
    {
        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public Guid ServiceTypeId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [MaxLength(1000)]
        public string? CustomerNotes { get; set; }
    }

    public class BookingResponseDto
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public BookingStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;   // human readable
        public DateTime ScheduledDate { get; set; }
        public string? CustomerNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Nested - so frontend doesn't need extra calls
        public VehicleResponseDto Vehicle { get; set; } = null!;
        public ServiceTypeResponseDto ServiceType { get; set; } = null!;
    }

    public class StatusHistoryDto
    {
        public BookingStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public string ChangedByRole { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; }
    }
    public class BookingDetailResponseDto : BookingResponseDto
    {
        // Full detail includes status history timeline
        public List<StatusHistoryDto> StatusHistory { get; set; } = new();
    }

    public class UpdateBookingRequestDto
    {
        public Guid VehicleId { get; set; }
        public Guid ServiceTypeId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string? CustomerNotes { get; set; }
    }
}
