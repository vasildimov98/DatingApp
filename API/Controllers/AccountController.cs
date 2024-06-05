using API.Dto;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly IUsersService usersService;
    private readonly ITokenService tokenService;

    public AccountController(
        IUsersService usersService,
        ITokenService tokenService)
    {
        this.usersService = usersService;
        this.tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await this.usersService
                .CheckUsernameAvailabilityAsync(registerDto.Username))
            return this.BadRequest("Username is taken!");

        var user = await this.usersService
                .RegisterUserAsync(registerDto.Username, registerDto.Password);

        return new UserDto
        {
            Username = registerDto.Username,
            Token = this.tokenService
                .GenerateToken(user)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) 
    {
        try
        {
            var user = await this.usersService
                    .GetUserAsyncBy(loginDto.Username, loginDto.Password);

            return new UserDto 
            {
                Username = user.UserName,
                Token = this.tokenService
                            .GenerateToken(user),
            };
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }    
    }
}
