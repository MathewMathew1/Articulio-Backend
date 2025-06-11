using Microsoft.AspNetCore.Mvc;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;

namespace ResearchScrapper.Api.Controller
{
    [ApiController]
    [Route("api/scientific-papers")]
    public class ScientificArticlesController : ControllerBase
    {
        private readonly ILogger<ScrapeController> _logger;
        private readonly IAggregateScientificPapersService _aggregateScientificPapers;
        private readonly IQuerySanitizationService _querySanitizationService;

        public ScientificArticlesController(ILogger<ScrapeController> logger,
        IAggregateScientificPapersService aggregateScientificPapers, IQuerySanitizationService querySanitizationService)
        {
            _logger = logger;
            _aggregateScientificPapers = aggregateScientificPapers;
            _querySanitizationService = querySanitizationService;
        }

        [HttpGet("search/tag/selective")]
        public async Task<ActionResult<Dictionary<SourceScientificArticleType, ArticleFetchResult>>> GetSelective(GetScientificArticlesDto data, CancellationToken cancellationToken,
        [FromQuery] int page = 1)
        {
            try
            {
                data.Query = _querySanitizationService.Sanitize(data.Query);
                var result = await _aggregateScientificPapers.ScrapeAsync(data.Query, data.Sources, cancellationToken, page, SearchTag.Tag);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpGet("search/author/selective")]
        public async Task<ActionResult<Dictionary<SourceScientificArticleType, ArticleFetchResult>>> GetSelectiveByAuthors(GetScientificArticlesDto data, CancellationToken cancellationToken,
        [FromQuery] int page = 1)
        {
            try
            {
                data.Query = _querySanitizationService.Sanitize(data.Query);
                

                var result = await _aggregateScientificPapers.ScrapeAsync(data.Query, data.Sources, cancellationToken, page, SearchTag.Author);
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
