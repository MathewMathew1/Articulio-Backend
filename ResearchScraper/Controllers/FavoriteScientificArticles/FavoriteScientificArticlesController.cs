using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ResearchScrapper.Api.MiddleWare;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;

namespace ResearchScrapper.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/scientific-favorites")]
    public class ScientificFavoriteArticlesController : ControllerBase
    {
        private readonly IFavoriteScientificArticleRepository _scientificFavoriteRepo;
        private readonly ILogger<ScientificFavoriteArticlesController> _logger;

        public ScientificFavoriteArticlesController(
            IFavoriteScientificArticleRepository scientificFavoriteRepo,
            ILogger<ScientificFavoriteArticlesController> logger)
        {
            _scientificFavoriteRepo = scientificFavoriteRepo;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] CreateFavoriteScientificArticle article)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var created = await _scientificFavoriteRepo.AddFavoriteAsync(article, userIdentity.Id);
                return CreatedAtAction(null, new { id = created.Id }, created);
            }
            catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                _logger.LogInformation("Duplicate (userId + URL) on AddFavorite: {Url}", article.Url);
                return Conflict(new { error = "Article with the same URL already exists for this user." });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in AddFavorite (Scientific)");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFavorite(string id, [FromBody] CreateFavoriteScientificArticle article)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var updated = await _scientificFavoriteRepo.UpdateFavoriteAsync(id, article, userIdentity.Id);

                if (updated is null)
                    return NotFound();

                return Ok(updated);
            }
            catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                _logger.LogInformation("Duplicate (userId + URL) on UpdateFavorite: {Url}", article.Url);
                return Conflict(new { error = "Article with the same URL already exists for this user." });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in UpdateFavorite (Scientific)");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavorite(string id)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var deleted = await _scientificFavoriteRepo.DeleteFavoriteAsync(id, userIdentity.Id);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in DeleteFavorite (Scientific)");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }
    }
}
