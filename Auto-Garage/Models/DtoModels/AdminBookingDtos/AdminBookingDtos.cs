using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using Auto_Garage.Models.DtoModels.ServiceTypeDtos;
using Auto_Garage.Models.DtoModels.VehicleDtos;
using System.ComponentModel.DataAnnotations;

namespace Auto_Garage.Models.DtoModels.AdminBookingDtos
{
    public class AdminBookingResponseDto
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public BookingStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string? CustomerNotes { get; set; }
        public string? AssignedMechanicId { get; set; }
        public string? AssignedMechanicName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public VehicleResponseDto Vehicle { get; set; } = null!;
        public ServiceTypeResponseDto ServiceType { get; set; } = null!;
    }

    public class AdminBookingDetailResponseDto : AdminBookingResponseDto
    {
        public List<AdminStatusHistoryDto> StatusHistory { get; set; } = new();
    }

    public class AdminStatusHistoryDto
    {
        public BookingStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public string ChangedByRole { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public class ConfirmBookingRequestDto
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
    public class AssignMechanicRequestDto
    {
        [Required]
        public string MechanicId { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
    public class ReassignMechanicRequestDto
    {
        [Required]
        public string MechanicId { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
    public class UpdateBookingStatusRequestDto
    {
        [Required]
        public BookingStatus NewStatus { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
    public class MechanicStaffDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
    public class MechanicUpdateStatusRequestDto
    {
        [Required]
        public BookingStatus NewStatus { get; set; }  

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}