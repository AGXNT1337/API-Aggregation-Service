using APIAggregation.Models;

namespace APIAggregation.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherInfo> GetCurrentWeatherAsync(string location);
    }

}
