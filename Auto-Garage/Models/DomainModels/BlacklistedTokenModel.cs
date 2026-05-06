namespace Auto_Garage.Models.DomainModels
{
    public class BlacklistedTokenModel
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public DateTime BlacklistedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
