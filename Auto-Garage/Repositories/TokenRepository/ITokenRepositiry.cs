using Auto_Garage.Models.DomainModels;

namespace Auto_Garage.Repositories.TokenRepository
{
    public interface ITokenRepositiry
    {
        string CreateJWTToken(AutoGarageUser user, List<string> roles);
        string GenerateRefreshToken();
    }
}
