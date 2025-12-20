using WebApp.API.Models;

namespace WebApp.API.Services;

public interface IWeatherService
{
    Task<WeatherData> GetWeatherAsync(string city);
    Task<ForecastData> GetForecastAsync(string city);
}