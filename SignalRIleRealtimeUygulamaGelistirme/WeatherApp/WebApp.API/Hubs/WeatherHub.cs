using Microsoft.AspNetCore.SignalR;
using WebApp.API.Services;

namespace WebApp.API.Hubs;

public class WeatherHub(IWeatherService weatherService, ILogger<WeatherHub> logger) : Hub
{
    public async Task SubscribeToCity(string city)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, city);
        logger.LogInformation($"Client {Context.ConnectionId} subscribed to {city}");
        
        var weather = await weatherService.GetWeatherAsync(city);
        await Clients.Caller.SendAsync("ReceiveWeather", weather);
    }
    
    public async Task UnsubscribeFromCity(string city)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, city);
        logger.LogInformation($"Client {Context.ConnectionId} unsubscribed from {city}");
    }
    
    public async Task GetCurrentWeather(string city)
    {
        var weather = await weatherService.GetWeatherAsync(city);
        await Clients.Caller.SendAsync("ReceiveWeather", weather);
    }
    
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}