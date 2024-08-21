using System.Text.Json.Serialization;

namespace APIAggregation.Models
{
    public class GitHubRepo
    {
        public string? Name { get; set; }
        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }
        public string? Visibility { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
