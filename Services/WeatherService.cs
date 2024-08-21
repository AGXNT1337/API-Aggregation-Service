namespace APIAggregation.Services
{
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using APIAggregation.Interfaces;
    using APIAggregation.Models;
    using Microsoft.Extensions.Configuration;

    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY") ?? throw new InvalidOperationException("OPENWEATHERMAP_API_KEY is not set.");

        }

        public async Task<WeatherInfo> GetCurrentWeatherAsync(string city)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}");

                var content = await response.Content.ReadAsStringAsync();
                var weatherResponse = JsonSerializer.Deserialize<WeatherInfo>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return weatherResponse;
            }
            catch (Exception)
            {
                return new WeatherInfo { Name = "Unavailable", Main = null, Weather = null};
            }
        }
    }

}
