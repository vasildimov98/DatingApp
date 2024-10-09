using System.Security.Cryptography;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDTO)
    {
        if (await DoesUserExistsAsync(registerDTO.UserName))
            return this.BadRequest("Username is taken!");

        var user = mapper.Map<AppUser>(registerDTO);

        user.UserName = registerDTO.UserName.ToLower();

        var result = await userManager.CreateAsync(user);

        if (!result.Succeeded)
            return this.BadRequest(result.Errors);

        return new UserDto
        {
            Username = user.UserName,
            Gender = user.Gender,
            KnownAs = user.KnownAs,
            Token = await tokenService.CreateToken(user),
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.Users
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => EF.Functions
                .Like(x.NormalizedUserName, loginDto.Username
                        .ToUpper()));

        if (user == null || user.UserName == null)
        {
            return this.Unauthorized("Invalid Username");
        }


        return new UserDto
        {
            Username = user.UserName,
            Gender = user.Gender,
            KnownAs = user.KnownAs,
            Token = await tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };
    }

    private async Task<bool> DoesUserExistsAsync(string username)
    {
        return await userManager.Users
            .AnyAsync(x => EF.Functions
                .Like(x.NormalizedUserName, username
                        .ToUpper()));
    }

}
