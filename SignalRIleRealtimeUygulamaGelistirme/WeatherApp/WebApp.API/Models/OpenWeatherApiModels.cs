using System.Text.Json.Serialization;

namespace WebApp.API.Models;

public class WeatherApiResponse
{
    [JsonPropertyName("current")]
    public CurrentWeather Current { get; set; } = new();
}

public class CurrentWeather
{
    [JsonPropertyName("temp_c")]
    public double TempC { get; set; }
    
    [JsonPropertyName("feelslike_c")]
    public double FeelslikeC { get; set; }
    
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
    
    [JsonPropertyName("pressure_mb")]
    public double PressureMb { get; set; }
    
    [JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }
    
    [JsonPropertyName("condition")]
    public WeatherCondition Condition { get; set; } = new();
}

public class WeatherCondition
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
}

public class WeatherApiForecastResponse
{
    [JsonPropertyName("forecast")]
    public ForecastWrapper Forecast { get; set; } = new();
}

public class ForecastWrapper
{
    [JsonPropertyName("forecastday")]
    public List<ForecastDay> Forecastday { get; set; } = new();
}

public class ForecastDay
{
    [JsonPropertyName("hour")]
    public List<HourWeather> Hour { get; set; } = new();
}

public class HourWeather
{
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;
    
    [JsonPropertyName("temp_c")]
    public double TempC { get; set; }
    
    [JsonPropertyName("condition")]
    public WeatherCondition Condition { get; set; } = new();
}