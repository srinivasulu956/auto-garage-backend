namespace Auto_Garage.Models.DtoModels
{
    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public string? UserName { get; set; }
        public List<string>? Roles { get; set; }

    }
}
