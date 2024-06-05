using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class UsersService : IUsersService
{
    private readonly ApplicationContext dbContext;

    public UsersService(ApplicationContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<bool> CheckUsernameAvailabilityAsync(string username)
    {
        return await this.dbContext.Users
                            .AnyAsync(x => EF.Functions
                                .Like(x.UserName, username));
    }

    public async Task<AppUser> GetUserAsyncBy(int id)
    {
        return await this.dbContext.Users
                            .FindAsync(id);
    }

    public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
    {
        return await this.dbContext.Users
                        .ToListAsync();
    }

    public async Task<AppUser> RegisterUserAsync(string username, string password)
    {
        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = username,
            PasswordHash = hmac
                .ComputeHash(Encoding.UTF8
                    .GetBytes(password)),
            PasswordSalt = hmac.Key,
        };

        await this.dbContext
                .AddAsync(user);
        await this.dbContext
                .SaveChangesAsync();
 
        return user;
    }

    public async Task<AppUser> GetUserAsyncBy(string username, string password)
    {
        var user = await this.dbContext.Users
                .SingleOrDefaultAsync(x => EF.Functions.Like(x.UserName, username));

        if (user == null)
        {
            throw new InvalidOperationException("Username is invalid!");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedPassword = hmac
                    .ComputeHash(Encoding.UTF8
                        .GetBytes(password));

        for (int i = 0; i < computedPassword.Length; i++)
        {
            if (computedPassword[i] != user.PasswordHash[i])
                throw new InvalidOperationException("Password is invalid!");
        }

        return user;
    }
}
