using APIAggregation.Models;

namespace APIAggregation.Interfaces
{
    public interface ITwitterService
    {
        Task<TwitterUserData> GetUserDataAsync(string username);
    }

}
