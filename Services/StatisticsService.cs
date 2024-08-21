using APIAggregation.Interfaces;
using APIAggregation.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace APIAggregation.Services
{
    public class StatisticsService : IStatisticsService
    {
        // Dictionary to store request statistics for each API
        private readonly ConcurrentDictionary<string, RequestStatsDetails> _apiStats = new();

        // Records a new request with its response time
        public void RecordRequest(string apiName, long responseTime)
        {
            // Get or create RequestStatsDetails for the given API name
            var stats = _apiStats.GetOrAdd(apiName, new RequestStatsDetails());
            // Increment the total request count and add the response time
            stats.TotalRequests++;
            stats.ResponseTimes.Add(responseTime);
        }

        // Retrieves the statistics for all APIs
        public RequestStatsResponse GetStatistics()
        {
            var result = new RequestStatsResponse
            {
                Stats = new Dictionary<string, RequestStatsDetails>()
            };

            // List of API names to track
            var apiNames = new[] { "github", "twitter", "weather" };

            // Process each API name
            foreach (var apiName in apiNames)
            {
                // Get or create RequestStatsDetails for the given API name
                var stats = _apiStats.GetOrAdd(apiName, new RequestStatsDetails());
                var responseTimes = stats.ResponseTimes;

                // Calculate the number of requests in each performance bucket
                var fastCount = responseTimes.Count(rt => rt < 100);
                var averageCount = responseTimes.Count(rt => rt >= 100 && rt <= 200);
                var slowCount = responseTimes.Count(rt => rt > 200);

                // Add statistics to the result dictionary
                result.Stats[apiName] = new RequestStatsDetails
                {
                    TotalRequests = stats.TotalRequests,
                    ResponseTimes = new List<long>(responseTimes),
                    Buckets = new Dictionary<string, int>
                    {
                        { "fast", fastCount },
                        { "average", averageCount },
                        { "slow", slowCount }
                    }
                };
            }

            return result;
        }
    }
}
