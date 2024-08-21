using APIAggregation.Models;

namespace APIAggregation.Interfaces
{
    public interface IAggregationService
    {
        Task<AggregatedData> GetAggregatedDataAsync(
            string github,
            string twitter,
            string location,
            string sortBy = "name",
            bool ascending = true,
            string? nameFilter = null,
            DateTime? createdAfter = null,
            DateTime? createdBefore = null,
            DateTime? updatedAfter = null,
            DateTime? updatedBefore = null);
    }
}
