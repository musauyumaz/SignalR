using System.Text.Json;
using WebApp.API.Models;

namespace WebApp.API.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherService> _logger;
    private readonly Dictionary<string, WeatherData> _cache = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public WeatherService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        try
        {
            var apiKey = _configuration["WeatherApi:ApiKey"];
            var url = $"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={city}&lang=tr";
            
            var response = await _httpClient.GetStringAsync(url);
            
            _logger.LogInformation($"API Response for {city}: {response.Substring(0, Math.Min(200, response.Length))}...");
            
            var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(response, _jsonOptions);

            if (apiResponse?.Current == null)
            {
                _logger.LogWarning($"No weather data received for {city}");
                return _cache.GetValueOrDefault(city) ?? new WeatherData { City = city };
            }

            var weather = new WeatherData
            {
                City = city,
                Temperature = Math.Round(apiResponse.Current.TempC, 1),
                FeelsLike = Math.Round(apiResponse.Current.FeelslikeC, 1),
                Humidity = apiResponse.Current.Humidity,
                Pressure = (int)Math.Round(apiResponse.Current.PressureMb),
                WindSpeed = Math.Round(apiResponse.Current.WindKph, 1),
                Description = apiResponse.Current.Condition.Text,
                Icon = apiResponse.Current.Condition.Icon,
                Timestamp = DateTime.UtcNow
            };

            _cache[city] = weather;
            _logger.LogInformation($"Weather fetched for {city}: {weather.Temperature}°C - {weather.Description}");
            
            return weather;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching weather for {city}");
            return _cache.GetValueOrDefault(city) ?? new WeatherData { City = city };
        }
    }

    public async Task<ForecastData> GetForecastAsync(string city)
    {
        try
        {
            var apiKey = _configuration["WeatherApi:ApiKey"];
            var url = $"https://api.weatherapi.com/v1/forecast.json?key={apiKey}&q={city}&days=1&lang=tr";
            
            var response = await _httpClient.GetStringAsync(url);
            var apiResponse = JsonSerializer.Deserialize<WeatherApiForecastResponse>(response, _jsonOptions);

            if (apiResponse?.Forecast?.Forecastday == null || !apiResponse.Forecast.Forecastday.Any())
            {
                return new ForecastData { City = city, Forecasts = new List<HourlyForecast>() };
            }

            var forecast = new ForecastData
            {
                City = city,
                Forecasts = apiResponse.Forecast.Forecastday[0].Hour.Select(h => new HourlyForecast
                {
                    Time = DateTime.Parse(h.Time),
                    Temperature = Math.Round(h.TempC, 1),
                    Description = h.Condition.Text,
                    Icon = h.Condition.Icon
                }).ToList()
            };

            return forecast;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching forecast for {city}");
            return new ForecastData { City = city, Forecasts = new List<HourlyForecast>() };
        }
    }
}