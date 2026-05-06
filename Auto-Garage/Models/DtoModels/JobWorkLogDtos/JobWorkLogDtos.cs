using System.ComponentModel.DataAnnotations;

namespace Auto_Garage.Models.DtoModels.JobWorkLogDtos
{
    public class AddWorkLogItemRequestDto
    {
        [Required]
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Range(0, 999999)]
        public decimal UnitCost { get; set; }
    }

    public class WorkLogItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public string MechanicId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }   // Quantity * UnitCost
        public DateTime CreatedAt { get; set; }
    }
}