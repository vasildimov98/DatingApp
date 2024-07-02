using API.DTOs;
using API.Entities;

namespace API.DTOs.Extensions;

public static class AppUserMapperExtensions
{
    public static UserDto MapUserDto(this AppUser user, string token)
    {
        return new UserDto
        {
            Username = user.UserName,
            Token = token,
        };
    }
}
