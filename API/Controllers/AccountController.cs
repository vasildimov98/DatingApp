using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDTO)
    {
        if (await DoesUserExistsAsync(registerDTO.UserName))
            return this.BadRequest("Username is taken!");

        return Ok();

        // using var hmac = new HMACSHA512();

        // var user = new AppUser
        // {
        //     UserName = registerDTO.UserName,
        //     PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
        //     PasswordSalt = hmac.Key,
        // };

        // var userDto =  user.MapUserDto(tokenService.CreateToken(user));

        // context.Users.Add(user);
        // await context.SaveChangesAsync();

        // return userDto;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(x => EF.Functions
                .Like(x.UserName
                    .ToLower(), loginDto.Username
                        .ToLower()));

        if (user == null)
        {
            return this.Unauthorized("Invalid Username");
        }

        var isPasswordCorrect = CheckPasswordAsync(user, loginDto.Password);

        if (!isPasswordCorrect)
        {
            return this.Unauthorized("Invalid Password");
        }

        return new UserDto 
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user),
        };
    }

    private async Task<bool> DoesUserExistsAsync(string username)
    {
        return await context.Users
            .AnyAsync(x => EF.Functions
                .Like(x.UserName
                    .ToLower(), username
                        .ToLower()));
    }

    private static bool CheckPasswordAsync(AppUser user, string password)
    {
        var hmac = new HMACSHA512(user.PasswordSalt);

        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        for (int i = 0; i < passwordHash.Length; i++)
        {
            if (passwordHash[i] != user.PasswordHash[i])
            {
                return false;
            }
        }
        
        return true;
    }
}
