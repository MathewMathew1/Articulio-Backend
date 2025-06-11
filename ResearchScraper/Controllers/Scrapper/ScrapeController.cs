using Microsoft.AspNetCore.Mvc;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;

namespace ResearchScrapper.Api.Controller
{
    [ApiController]
    [Route("api/articles")]
    public class ScrapeController : ControllerBase
    {

        private readonly IAggregateService _aggregateScraper;
        private readonly ILogger<ScrapeController> _logger;

        public ScrapeController(ILogger<ScrapeController> logger, IAggregateService aggregateScraperService)
        {

            _logger = logger;
            _aggregateScraper = aggregateScraperService;
        }



        [HttpGet("selective")]
        public async Task<ActionResult<Dictionary<SourceType, ArticleDtoResult>>> GetSelective(GetArticlesDto data,
        CancellationToken cancellationToken,
            [FromQuery] int page = 1
         )
        {
            try
            {
                var result = await _aggregateScraper.ScrapeAsync(data.Query, data.Sources, page, cancellationToken);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }
    }
}
