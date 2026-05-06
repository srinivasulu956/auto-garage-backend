using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auto_Garage.Models.DomainModels.JobWorkLogModel
{
    public class JobWorkLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public string MechanicId { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 999999)]
        public decimal UnitCost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}