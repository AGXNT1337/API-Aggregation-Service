namespace APIAggregation.Models
{
    public class RequestStatsDetails
    {
        public int TotalRequests { get; set; }
        public List<long> ResponseTimes { get; set; } = new List<long>();
        public Dictionary<string, int> Buckets { get; set; } = new Dictionary<string, int>();
    }
}
