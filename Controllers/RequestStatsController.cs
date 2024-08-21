namespace APIAggregation.Controllers
{
    using APIAggregation.Interfaces;
    using APIAggregation.Models;
    using Microsoft.AspNetCore.Mvc;
    using System;

    [Route("api/aggregation")]
    [ApiController]
    public class RequestStatsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public RequestStatsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("request-stats")]
        public ActionResult<RequestStatsResponse> GetStats()
        {
            try
            {
                // Retrieve statistics from the service
                var statsResponse = _statisticsService.GetStatistics();

                // Return the statistics response directly
                return Ok(statsResponse);
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error if an exception occurs
                return StatusCode(500, new
                {
                    Message = "Data is unavailable",
                    Error = ex.Message
                });
            }
        }
    }
}
