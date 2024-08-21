using APIAggregation.Interfaces;
using APIAggregation.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace APIAggregation.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _accessToken;

        public GitHubService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _accessToken = Environment.GetEnvironmentVariable("GITHUB_ACCESS_TOKEN") ?? throw new InvalidOperationException("GITHUB_ACCESS_TOKEN is not set.");
        }

        public async Task<IEnumerable<GitHubRepo>> GetUserRepositoriesAsync(string username)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/users/{username}/repos");
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                var response = await _httpClient.SendAsync(request);

                var content = await response.Content.ReadAsStringAsync();
                var repos = JsonSerializer.Deserialize<List<GitHubRepo>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return repos;
            }
            catch (Exception)
            {
                return new List<GitHubRepo> {
                           new GitHubRepo {
                               Name = "Unavailable",
                               HtmlUrl = "Unavailable",
                               Visibility = "Unavailable",
                               CreatedAt = null,
                               UpdatedAt = null
                }};
            }
        }


    }

}
