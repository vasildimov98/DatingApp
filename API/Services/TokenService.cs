using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService(IConfiguration config, UserManager<AppUser> userManager) : ITokenService
{
    public async Task<string> CreateToken(AppUser user)
    {
        var tokenKey = config["TokenKey"] 
            ?? throw new ArgumentNullException("Cannot access tokenKey from app setting.");

        if (tokenKey.Length < 64)
            throw new ArgumentOutOfRangeException("Token needs to be longer!");

         var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

         if (user.UserName == null)
            throw new Exception("Username is invalid");

         var claims = new List<Claim>
         {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
         };

         var roles = await userManager.GetRolesAsync(user);

         claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));

         var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

         var tokenDescriptor  = new SecurityTokenDescriptor
         {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds,
         };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
