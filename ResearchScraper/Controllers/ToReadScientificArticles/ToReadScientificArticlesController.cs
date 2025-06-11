using Microsoft.AspNetCore.Mvc;
using ResearchScrapper.Api.MiddleWare;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;

namespace ResearchScrapper.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/scientific-toreads")]
    public class ScientificToReadArticlesController : ControllerBase
    {
        private readonly IToReadScientificArticleRepository _scientificToReadRepo;
        private readonly ILogger<ScientificToReadArticlesController> _logger;

        public ScientificToReadArticlesController(
            IToReadScientificArticleRepository scientificToReadRepo,
            ILogger<ScientificToReadArticlesController> logger)
        {
            _scientificToReadRepo = scientificToReadRepo;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddToRead([FromBody] CreateToReadScientificArticle article)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var created = await _scientificToReadRepo.AddToReadAsync(article, userIdentity.Id);
                return CreatedAtAction(null, new { id = created.Id }, created);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in AddToRead (Scientific)");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateToRead(string id, [FromBody] CreateToReadScientificArticle article)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var updated = await _scientificToReadRepo.UpdateToReadAsync(id, article, userIdentity.Id);

                if (updated is null)
                    return NotFound();

                return Ok(updated);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in UpdateToRead (Scientific)");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToRead(string id)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var deleted = await _scientificToReadRepo.DeleteToReadAsync(id, userIdentity.Id);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in DeleteToRead (Scientific)");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }
    }
}
