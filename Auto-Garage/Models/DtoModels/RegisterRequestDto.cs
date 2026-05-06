using System.ComponentModel.DataAnnotations;

namespace Auto_Garage.Models.DtoModels
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string? FirstName { get; set; }
        [Required]
        [MaxLength(100)]
        public string? LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Required]
        public string[]? Roles { get; set; }
    }
}
