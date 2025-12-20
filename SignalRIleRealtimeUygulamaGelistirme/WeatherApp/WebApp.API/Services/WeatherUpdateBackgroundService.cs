using Microsoft.AspNetCore.SignalR;
using WebApp.API.Hubs;

namespace WebApp.API.Services;

public class WeatherUpdateBackgroundService(IServiceProvider serviceProvider, ILogger<WeatherUpdateBackgroundService> logger) : BackgroundService
{
    private readonly List<string> _popularCities = new() 
    { 
        "Istanbul", "Ankara", "Izmir", "Antalya", "Bursa" , "Eskisehir"
    };


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Weather update service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<WeatherHub>>();
                var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();

                foreach (var city in _popularCities)
                {
                    var weather = await weatherService.GetWeatherAsync(city);
                    await hubContext.Clients.Group(city).SendAsync("ReceiveWeather", weather, stoppingToken);
                    logger.LogInformation($"Updated weather for {city}");
                }

                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in weather update service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}