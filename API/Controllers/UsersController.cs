using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext dbcontext = dbContext;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        var users = await this.dbcontext.Users.ToListAsync();

        return users;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        var user = await this.dbcontext.Users.FindAsync(id);

        return user;
    }
}
