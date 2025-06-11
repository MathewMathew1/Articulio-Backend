using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ResearchScrapper.Api.MiddleWare;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;

namespace ResearchScrapper.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/favorites")]
    public class FavoriteArticlesController : ControllerBase
    {
        private readonly IFavoriteArticleRepository _favoriteRepo;
        private readonly ILogger<FavoriteArticlesController> _logger;

        public FavoriteArticlesController(IFavoriteArticleRepository favoriteRepo, ILogger<FavoriteArticlesController> logger)
        {
            _favoriteRepo = favoriteRepo;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] CreateArticle article)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;

                var created = await _favoriteRepo.AddFavoriteAsync(article, userIdentity.Id);
                return CreatedAtAction(null, new { id = created.Id }, created);
            }
            catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                _logger.LogInformation("Duplicate URL on AddFavorite: {Url}", article.Url);
                return Conflict(new { error = "Article with the same URL already exists." });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in AddFavorite");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateFavorite([FromBody] CreateArticle article)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var updated = await _favoriteRepo.UpdateFavoriteAsync(article, userIdentity.Id);

                if (updated is null)
                    return NotFound();

                return Ok(updated); 
            }
            catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                _logger.LogInformation("Duplicate URL on UpdateFavorite: {Url}", article.Url);
                return Conflict(new { error = "Article with the same URL already exists." });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in UpdateFavorite");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavorite(string id)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var deleted = await _favoriteRepo.DeleteFavoriteAsync(id, userIdentity.Id);
                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in DeleteFavorite");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }
    }
}
