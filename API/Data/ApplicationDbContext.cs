using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions options)
     : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
}
