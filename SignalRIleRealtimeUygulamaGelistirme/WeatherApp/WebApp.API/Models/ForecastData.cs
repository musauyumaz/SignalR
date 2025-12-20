namespace WebApp.API.Models;

public record ForecastData
{
    public string City { get; set; }
    public List<HourlyForecast> Forecasts { get; set; }
}

public record HourlyForecast
{
    public DateTime Time { get; set; }
    public double Temperature { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}