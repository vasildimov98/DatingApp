using API.Entities;

namespace API.Services;

public interface IUsersService
{
    Task<bool> CheckUsernameAvailabilityAsync(string username);

    Task<AppUser> GetUserAsyncBy(int id);
    
    Task<AppUser> GetUserAsyncBy(string username, string password);

    Task<IEnumerable<AppUser>> GetAllUsersAsync();

    Task<AppUser> RegisterUserAsync(string username, string password);
}
