namespace APIAggregation.Interfaces
{
    using APIAggregation.Models;

    public interface IStatisticsService
    {
        void RecordRequest(string apiName, long responseTime);
        RequestStatsResponse GetStatistics();
    }
}
