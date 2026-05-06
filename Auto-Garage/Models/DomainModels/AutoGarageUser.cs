using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Auto_Garage.Models.DomainModels
{
    public class AutoGarageUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string? LastName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public string ThemePreference { get; set; } = "light";
    }
}
