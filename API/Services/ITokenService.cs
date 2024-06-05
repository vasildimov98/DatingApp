using API.Entities;

namespace API.Services;

public interface ITokenService
{
    string GenerateToken(AppUser user);
}
