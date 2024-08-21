using APIAggregation.Models;

namespace APIAggregation.Interfaces
{
    public interface IGitHubService
    {
        Task<IEnumerable<GitHubRepo>> GetUserRepositoriesAsync(string username);
    }
}
