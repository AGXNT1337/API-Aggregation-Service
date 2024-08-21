using APIAggregation.Services;

namespace APIAggregation.Models
{
    public class WeatherInfo
    {
        public string? Name { get; set; }
        public WeatherMain? Main { get; set; }
        public IEnumerable<WeatherDescription>? Weather { get; set; }
    }
}
