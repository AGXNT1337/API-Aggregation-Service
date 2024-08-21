namespace APIAggregation.Services
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using APIAggregation.Interfaces;
    using APIAggregation.Models;
    using System.Net;

    public class TwitterService : ITwitterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _bearerToken;

        public TwitterService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _bearerToken = Environment.GetEnvironmentVariable("TWITTER_BEARER_TOKEN") ?? throw new InvalidOperationException("TWITTER_BEARER_TOKEN is not set.");

        }

        public async Task<TwitterUserData> GetUserDataAsync(string username)
        {
            try
            {
                var requestUrl = $"https://api.twitter.com/2/users/by/username/{username}";
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);

                var response = await _httpClient.SendAsync(request);


                var content = await response.Content.ReadAsStringAsync();
                var twitterResponse = JsonSerializer.Deserialize<TwitterApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return twitterResponse.Data;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Handle rate limiting (Twitter Free API has serious issues with this one)
                return new TwitterUserData { Id = "Unavailable", Name = "Unavailable", Username = "Unavailable" };
            }

            catch (Exception)
            {
                return new TwitterUserData { Id = "Unavailable", Name = "Unavailable", Username = "Unavailable" };
            }

        }
    }
}
