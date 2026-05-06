namespace Auto_Garage.Models.DomainModels
{
    public class RefreshTokenModel
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;

        // Navigation property
        public AutoGarageUser? User { get; set; }
    }
}
