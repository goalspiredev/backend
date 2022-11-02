using GoalspireBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace GoalspireBackend.Data;

public class DataContext : DbContext
{
    private readonly IConfiguration _configuration;

    public DataContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(_configuration.GetConnectionString("Postgres"));
    }

    public DbSet<Goal> Goals { get; set; } = null!;
}