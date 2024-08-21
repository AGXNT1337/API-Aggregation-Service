using Xunit;
using Moq;
using APIAggregation.Interfaces;
using APIAggregation.Services;
using APIAggregation.Models;
using Microsoft.Extensions.Caching.Memory;

namespace APIAggregation.Tests
{
    public class AggregationServiceTests
    {
        private readonly Mock<IGitHubService> _mockGitHubService;
        private readonly Mock<ITwitterService> _mockTwitterService;
        private readonly Mock<IWeatherService> _mockWeatherService;
        private readonly Mock<IStatisticsService> _mockStatisticsService;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly AggregationService _aggregationService;

        public AggregationServiceTests()
        {
            _mockGitHubService = new Mock<IGitHubService>();
            _mockTwitterService = new Mock<ITwitterService>();
            _mockWeatherService = new Mock<IWeatherService>();
            _mockStatisticsService = new Mock<IStatisticsService>();
            _mockCache = new Mock<IMemoryCache>();

            _aggregationService = new AggregationService(
                _mockGitHubService.Object,
                _mockTwitterService.Object,
                _mockWeatherService.Object,
                _mockStatisticsService.Object,
                _mockCache.Object
            );
        }

        [Fact]
        public async Task GetAggregatedDataAsync_ReturnsAggregatedData()
        {
            // Arrange
            var gitHubRepos = new List<GitHubRepo>
            {
                new GitHubRepo
                {
                    Name = "TestRepo",
                    HtmlUrl = "https://github.com/testrepo",
                    Visibility = "public",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };
            var twitterUser = new TwitterUserData { Id = "1", Name = "TestUser", Username = "testuser" };
            var weatherInfo = new WeatherInfo
            {
                Name = "TestCity",
                Main = new WeatherMain { Temp = 20.0, Humidity = 5 },
                Weather = new List<WeatherDescription> { new WeatherDescription { Description = "Clear" } }
            };

            _mockGitHubService.Setup(s => s.GetUserRepositoriesAsync(It.IsAny<string>())).ReturnsAsync(gitHubRepos);
            _mockTwitterService.Setup(s => s.GetUserDataAsync(It.IsAny<string>())).ReturnsAsync(twitterUser);
            _mockWeatherService.Setup(s => s.GetCurrentWeatherAsync(It.IsAny<string>())).ReturnsAsync(weatherInfo);

            _mockStatisticsService.Setup(s => s.RecordRequest(It.IsAny<string>(), It.IsAny<long>()));

            // Mock cache to return null, indicating cache miss
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<AggregatedData>.IsAny)).Returns(false);

            // Act
            var result = await _aggregationService.GetAggregatedDataAsync("testuser", "testlocation", "name", "nameFilter", true, null, null, null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.GitHub);
            Assert.NotEmpty(result.GitHub); // Ensure the GitHub list is not empty

            var gitHubRepoList = result.GitHub.ToList();
            Assert.Equal("TestRepo", gitHubRepoList[0].Name);
            Assert.Equal("TestUser", result.Twitter.Name);
            Assert.Equal("TestCity", result.Weather.Name);

            // Verify cache was set
            _mockCache.Verify(c => c.Set(It.IsAny<string>(), result, It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }

        [Fact]
        public async Task GetAggregatedDataAsync_WhenGitHubFails_ReturnsFallbackData()
        {
            // Arrange
            _mockGitHubService.Setup(service => service.GetUserRepositoriesAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException());
            _mockTwitterService.Setup(service => service.GetUserDataAsync(It.IsAny<string>()))
                .ReturnsAsync(new TwitterUserData { Id = "123", Username = "testuser", Name = "Test User" });
            _mockWeatherService.Setup(service => service.GetCurrentWeatherAsync(It.IsAny<string>()))
                .ReturnsAsync(new WeatherInfo { Name = "Athens", Main = new WeatherMain { Temp = 25.0 }, Weather = new List<WeatherDescription> { new WeatherDescription { Description = "Clear" } } });

            _mockStatisticsService.Setup(s => s.RecordRequest(It.IsAny<string>(), It.IsAny<long>()));

            // Mock cache to return null, indicating cache miss
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<AggregatedData>.IsAny)).Returns(false);

            // Act
            var result = await _aggregationService.GetAggregatedDataAsync("testuser", "testlocation", "name", "nameFilter", true, null, null, null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.GitHub);
            Assert.NotEmpty(result.GitHub); // Ensure the GitHub list is not empty

            var gitHubRepoList = result.GitHub.ToList();
            Assert.Equal("Unavailable", gitHubRepoList[0].Name);
            Assert.Equal("testuser", result.Twitter.Username);
            Assert.Equal("Athens", result.Weather.Name);

            // Verify cache was set
            _mockCache.Verify(c => c.Set(It.IsAny<string>(), result, It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }

        [Fact]
        public async Task GetAggregatedDataAsync_WhenTwitterFails_ReturnsFallbackData()
        {
            // Arrange
            _mockGitHubService.Setup(service => service.GetUserRepositoriesAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<GitHubRepo> { new GitHubRepo { Name = "TestRepo", HtmlUrl = "http://example.com" } });
            _mockTwitterService.Setup(service => service.GetUserDataAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException());
            _mockWeatherService.Setup(service => service.GetCurrentWeatherAsync(It.IsAny<string>()))
                .ReturnsAsync(new WeatherInfo { Name = "Athens", Main = new WeatherMain { Temp = 25.0 }, Weather = new List<WeatherDescription> { new WeatherDescription { Description = "Clear" } } });

            _mockStatisticsService.Setup(s => s.RecordRequest(It.IsAny<string>(), It.IsAny<long>()));

            // Mock cache to return null, indicating cache miss
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<AggregatedData>.IsAny)).Returns(false);

            // Act
            var result = await _aggregationService.GetAggregatedDataAsync("testuser", "testlocation", "name", "nameFilter", true, null, null, null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.GitHub);
            Assert.NotEmpty(result.GitHub); // Ensure the GitHub list is not empty

            var gitHubRepoList = result.GitHub.ToList();
            Assert.Equal("TestRepo", gitHubRepoList[0].Name);
            Assert.Equal("Unavailable", result.Twitter.Username);
            Assert.Equal("Athens", result.Weather.Name);

            // Verify cache was set
            _mockCache.Verify(c => c.Set(It.IsAny<string>(), result, It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }

        [Fact]
        public async Task GetAggregatedDataAsync_WhenWeatherFails_ReturnsFallbackData()
        {
            // Arrange
            _mockGitHubService.Setup(service => service.GetUserRepositoriesAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<GitHubRepo> { new GitHubRepo { Name = "TestRepo", HtmlUrl = "http://example.com" } });
            _mockTwitterService.Setup(service => service.GetUserDataAsync(It.IsAny<string>()))
                .ReturnsAsync(new TwitterUserData { Id = "123", Username = "testuser", Name = "Test User" });
            _mockWeatherService.Setup(service => service.GetCurrentWeatherAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException());

            _mockStatisticsService.Setup(s => s.RecordRequest(It.IsAny<string>(), It.IsAny<long>()));

            // Mock cache to return null, indicating cache miss
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<AggregatedData>.IsAny)).Returns(false);

            // Act
            var result = await _aggregationService.GetAggregatedDataAsync("testuser", "testlocation", "name", "nameFilter", true, null, null, null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.GitHub);
            Assert.NotEmpty(result.GitHub); // Ensure the GitHub list is not empty

            var gitHubRepoList = result.GitHub.ToList();
            Assert.Equal("TestRepo", gitHubRepoList[0].Name);
            Assert.Equal("testuser", result.Twitter.Username);
            Assert.Equal("Unavailable", result.Weather.Name); // Since fallback data returns null name

            // Verify cache was set
            _mockCache.Verify(c => c.Set(It.IsAny<string>(), result, It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }
    }
}
