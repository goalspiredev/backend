using GoalspireBackend.Data;
using GoalspireBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoalspireBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly DataContext _data;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DataContext data)
        {
            _logger = logger;
            _data = data;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<List<Goal>> Get()
        {
            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                CreatedAt = default,
                UpdatedAt = default,
                UserId = default,
                Type = GoalType.Goal,
                Title = "testing",
                Content = "content content",
                Priority = 5,
                EndsAt = DateTime.UtcNow + TimeSpan.FromDays(1),
                IsCompleted = false,
                IsPublic = false
            };

            await _data.Goals.AddAsync(goal);
            await _data.SaveChangesAsync();

            return await _data.Goals.ToListAsync();
        }
    }
}