﻿using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
     : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
}
