using APIAggregation.Interfaces;
using APIAggregation.Models;
using System.Diagnostics;

namespace APIAggregation.Services
{
    public class AggregationService : IAggregationService
    {
        private readonly IGitHubService _gitHubService;
        private readonly ITwitterService _twitterService;
        private readonly IWeatherService _weatherService;
        private readonly IStatisticsService _statisticsService;

        public AggregationService(
            IGitHubService gitHubService,
            ITwitterService twitterService,
            IWeatherService weatherService,
            IStatisticsService statisticsService)
        {
            _gitHubService = gitHubService;
            _twitterService = twitterService;
            _weatherService = weatherService;
            _statisticsService = statisticsService;
        }

        public async Task<AggregatedData> GetAggregatedDataAsync(
            string github,
            string twitter,
            string location,
            string sortBy = "name",
            bool ascending = true,
            string? nameFilter = null,
            DateTime? createdAfter = null,
            DateTime? createdBefore = null,
            DateTime? updatedAfter = null,
            DateTime? updatedBefore = null)
        {
            var aggregatedData = new AggregatedData
            {
                GitHub = new List<GitHubRepo>(),
                Twitter = new TwitterUserData { Id = "Unavailable", Username = "Unavailable", Name = "Unavailable" },
                Weather = new WeatherInfo
                {
                    Name = "Unavailable",
                    Main = new WeatherMain { Temp = double.NaN },
                    Weather = new List<WeatherDescription> { new WeatherDescription { Description = "Unavailable" } }
                }
            };

            var gitHubTask = Task.Run(async () =>
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var gitHubData = await _gitHubService.GetUserRepositoriesAsync(github);
                    gitHubData = FilterGitHubData(gitHubData, nameFilter, createdAfter, createdBefore, updatedAfter, updatedBefore);
                    aggregatedData.GitHub = SortGitHubData(gitHubData, sortBy, ascending).ToList();
                }
                catch (Exception)
                {
                    aggregatedData.GitHub.Add(new GitHubRepo { Name = "Unavailable", HtmlUrl = "Unavailable" });
                }
                finally
                {
                    stopwatch.Stop();
                    _statisticsService.RecordRequest("github", stopwatch.ElapsedMilliseconds);
                }
            });

            var twitterTask = Task.Run(async () =>
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    aggregatedData.Twitter = await _twitterService.GetUserDataAsync(twitter) ?? new TwitterUserData { Id = "Unavailable", Username = "Unavailable", Name = "Unavailable" };
                }
                catch (Exception)
                {
                    aggregatedData.Twitter = new TwitterUserData { Id = "Unavailable", Username = "Unavailable", Name = "Unavailable" };
                }
                finally
                {
                    stopwatch.Stop();
                    _statisticsService.RecordRequest("twitter", stopwatch.ElapsedMilliseconds);
                }
            });

            var weatherTask = Task.Run(async () =>
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    aggregatedData.Weather = await _weatherService.GetCurrentWeatherAsync(location) ?? new WeatherInfo
                    {
                        Name = "Unavailable",
                        Main = new WeatherMain { Temp = double.NaN },
                        Weather = new List<WeatherDescription> { new WeatherDescription { Description = "Unavailable" } }
                    };
                }
                catch (Exception)
                {
                    aggregatedData.Weather = new WeatherInfo
                    {
                        Name = "Unavailable",
                        Main = new WeatherMain { Temp = double.NaN },
                        Weather = new List<WeatherDescription> { new WeatherDescription { Description = "Unavailable" } }
                    };
                }
                finally
                {
                    stopwatch.Stop();
                    _statisticsService.RecordRequest("weather", stopwatch.ElapsedMilliseconds);
                }
            });

            await Task.WhenAll(gitHubTask, twitterTask, weatherTask);

            return aggregatedData;
        }


        private IEnumerable<GitHubRepo> FilterGitHubData(IEnumerable<GitHubRepo> repos,
            string? nameFilter,
            DateTime? createdAfter,
            DateTime? createdBefore,
            DateTime? updatedAfter,
            DateTime? updatedBefore)
        {
            try
            {
                if (!string.IsNullOrEmpty(nameFilter))
                {
                    repos = repos.Where(r => r.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));
                }

                if (createdAfter.HasValue)
                {
                    repos = repos.Where(r => r.CreatedAt >= createdAfter.Value);
                }

                if (createdBefore.HasValue)
                {
                    repos = repos.Where(r => r.CreatedAt <= createdBefore.Value);
                }

                if (updatedAfter.HasValue)
                {
                    repos = repos.Where(r => r.UpdatedAt >= updatedAfter.Value);
                }

                if (updatedBefore.HasValue)
                {
                    repos = repos.Where(r => r.UpdatedAt <= updatedBefore.Value);
                }

                return repos;
            }
            catch (Exception)
            {
                return repos;
            }
        }

        private IEnumerable<GitHubRepo> SortGitHubData(IEnumerable<GitHubRepo> repos, string sortBy, bool ascending)
        {
            return sortBy?.ToLower() switch
            {
                "name" => ascending ? repos.OrderBy(r => r.Name) : repos.OrderByDescending(r => r.Name),
                "createddate" => ascending ? repos.OrderBy(r => r.CreatedAt) : repos.OrderByDescending(r => r.CreatedAt),
                "updateddate" => ascending ? repos.OrderBy(r => r.UpdatedAt) : repos.OrderByDescending(r => r.UpdatedAt),
                _ => repos
            };
        }
    }
}
