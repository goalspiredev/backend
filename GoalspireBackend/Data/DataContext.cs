using GoalspireBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GoalspireBackend.Data;

public class DataContext : IdentityDbContext<User>
{
    private readonly IConfiguration _configuration;

    public DataContext(DbContextOptions options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(_configuration.GetConnectionString("SqlConnection"));
    }

    public DbSet<Goal> Goals { get; set; } = null!;
    public DbSet<Settings> Settings { get; set; } = null!;
    public DbSet<NotificationSubscription> NotificationSubscriptions { get; set; } = null!;
}