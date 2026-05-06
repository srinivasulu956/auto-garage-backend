using System.ComponentModel.DataAnnotations;

namespace Auto_Garage.Models.DtoModels.ServiceTypeDtos
{
    public class ServiceTypeResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public double EstimatedHours { get; set; }
        public decimal BookedBasePrice { get; set; }
    }

    public class AddServiceTypeRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 999999)]
        public decimal BasePrice { get; set; }

        [Required]
        [Range(0.5, 72)]
        public double EstimatedHours { get; set; }
    }
}