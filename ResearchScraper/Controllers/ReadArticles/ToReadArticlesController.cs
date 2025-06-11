using Microsoft.AspNetCore.Mvc;
using ResearchScrapper.Api.MiddleWare;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;

namespace ResearchScrapper.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/toreads")]
    public class ToReadArticlesController : ControllerBase
    {
        private readonly IToReadArticleRepository _toReadRepo;
        private readonly ILogger<ToReadArticlesController> _logger;

        public ToReadArticlesController(IToReadArticleRepository toReadRepo, ILogger<ToReadArticlesController> logger)
        {
            _toReadRepo = toReadRepo;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddToRead([FromBody] CreateReadArticle article)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var created = await _toReadRepo.AddToReadAsync(article, userIdentity.Id);
                return CreatedAtAction(null, new { id = created.Id }, created);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in AddToRead");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }


        [HttpPut]
        public async Task<IActionResult> UpdateToRead([FromBody] CreateReadArticle article)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var updated = await _toReadRepo.UpdateToReadAsync(article, userIdentity.Id);

                if (updated is null)
                    return NotFound();

                return Ok(updated);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in UpdateToRead");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToRead(string id)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var deleted = await _toReadRepo.DeleteToReadAsync(id, userIdentity.Id);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in DeleteToRead");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }
    }
}
