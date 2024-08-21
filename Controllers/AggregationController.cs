using APIAggregation.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace APIAggregation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AggregationController : ControllerBase
    {
        private readonly IAggregationService _aggregationService;

        public AggregationController(IAggregationService aggregationService)
        {
            _aggregationService = aggregationService;
        }

        [HttpGet("aggregate")]
        public async Task<IActionResult> GetAggregatedData(
            [FromQuery] string github,
            [FromQuery] string twitter,
            [FromQuery] string location,
            [FromQuery] string sortBy = "name",
            [FromQuery] bool ascending = true,
            [FromQuery] string? nameFilter = null,
            [FromQuery] DateTime? createdAfter = null,
            [FromQuery] DateTime? createdBefore = null,
            [FromQuery] DateTime? updatedAfter = null,
            [FromQuery] DateTime? updatedBefore = null)
        {
            try
            {
                var aggregatedData = await _aggregationService.GetAggregatedDataAsync(
                    github,
                    twitter,
                    location,
                    sortBy,
                    ascending,
                    nameFilter,
                    createdAfter,
                    createdBefore,
                    updatedAfter,
                    updatedBefore);
                return Ok(aggregatedData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }

}
