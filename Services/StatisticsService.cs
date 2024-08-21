using APIAggregation.Interfaces;
using APIAggregation.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class StatisticsService : IStatisticsService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, RequestStatsDetails> _apiStats = new();

    public StatisticsService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void RecordRequest(string apiName, long responseTime)
    {
        var stats = _apiStats.GetOrAdd(apiName, new RequestStatsDetails());

        // Increment the total request count and add the response time
        stats.TotalRequests++;
        stats.ResponseTimes.Add(responseTime);

        // Invalidate cache entry for the specific API stats
        _cache.Remove($"api_stats_{apiName}");
    }

    public RequestStatsResponse GetStatistics()
    {
        var result = new RequestStatsResponse
        {
            Stats = new Dictionary<string, RequestStatsDetails>()
        };

        var apiNames = new[] { "github", "twitter", "weather" };

        foreach (var apiName in apiNames)
        {
            string cacheKey = $"api_stats_{apiName}";

            // Try to get the cached data
            if (!_cache.TryGetValue(cacheKey, out RequestStatsDetails stats))
            {
                // If cache miss, calculate stats
                stats = _apiStats.GetOrAdd(apiName, new RequestStatsDetails());

                var responseTimes = stats.ResponseTimes;

                var fastCount = responseTimes.Count(rt => rt < 100);
                var averageCount = responseTimes.Count(rt => rt >= 100 && rt <= 200);
                var slowCount = responseTimes.Count(rt => rt > 200);

                // Update the stats object with calculated values
                stats = new RequestStatsDetails
                {
                    TotalRequests = stats.TotalRequests,
                    ResponseTimes = responseTimes.ToList(),
                    Buckets = new Dictionary<string, int>
                    {
                        { "fast", fastCount },
                        { "average", averageCount },
                        { "slow", slowCount }
                    }
                };

                // Cache the result with an expiration time
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                };

                _cache.Set(cacheKey, stats, cacheEntryOptions);
            }

            result.Stats[apiName] = stats;
        }

        return result;
    }
}
