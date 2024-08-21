namespace APIAggregation.Models
{
    public class RequestStatsResponse
    {
        public Dictionary<string, RequestStatsDetails> Stats { get; set; } = new Dictionary<string, RequestStatsDetails>();
    }
}
