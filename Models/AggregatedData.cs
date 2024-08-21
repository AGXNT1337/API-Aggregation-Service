
namespace APIAggregation.Models
{
    public class AggregatedData
    {
        public List<GitHubRepo> GitHub { get; set; }
        public TwitterUserData Twitter { get; set; }
        public WeatherInfo Weather { get; set; }
    }
}
