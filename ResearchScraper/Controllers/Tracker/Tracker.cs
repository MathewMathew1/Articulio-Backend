using Microsoft.AspNetCore.Mvc;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;

namespace ResearchScrapper.Api.Controller
{
    [ApiController]
    [Route("api/tracker")]
    public class TrackerController : ControllerBase
    {

        private readonly ILogger<TrackerController> _logger;
        private readonly IIpAbuseService _ipAbuseService;
        private readonly ArticleViewTrackerService _viewTracker;
        private readonly ScientificArticleDownloadTrackerService _scientificArticleDownloadTrackerService;
        private readonly ScientificArticleDoiTrackerService _scientificArticleDoiTrackerService;
        private readonly IUserRepository _userRepository;

        public TrackerController(ILogger<TrackerController> logger, IIpAbuseService ipAbuseService, ArticleViewTrackerService viewTrackerService,
        ScientificArticleDownloadTrackerService scientificArticleDownloadTrackerService, ScientificArticleDoiTrackerService scientificArticleDoiTrackerService,
        IUserRepository userRepository
        )
        {
            _ipAbuseService = ipAbuseService;
            _logger = logger;
            _viewTracker = viewTrackerService;
            _scientificArticleDownloadTrackerService = scientificArticleDownloadTrackerService;
            _scientificArticleDoiTrackerService = scientificArticleDoiTrackerService;
            _userRepository = userRepository;
        }

        [HttpGet("mostPopular")]
        public async Task<ActionResult<IEnumerable<MostPopularArticlesDto>>> GetMostPopularArticles([FromQuery] int page = 1)
        {
            try
            {
                if (page < 1)
                    page = 1;

                int offset = (page - 1) * 5;
                var mostPopularArticles = await _viewTracker.GetTopWithMetadataAsync(offset, 5);

                return mostPopularArticles.ToList();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpGet("mostPopular/scientific/downloaded")]
        public async Task<ActionResult<IEnumerable<MostPopularArticlesDto>>> GetMostPopularDownloadedScientificArticles([FromQuery] int page = 1)
        {
            try
            {
                if (page < 1)
                    page = 1;

                int offset = (page - 1) * 5;
                var mostPopularArticles = await _scientificArticleDownloadTrackerService.GetTopWithMetadataAsync(offset, 5);

                return mostPopularArticles.ToList();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpGet("mostPopular/scientific/doi")]
        public async Task<ActionResult<IEnumerable<MostPopularArticlesDto>>> GetMostPopularDoiScientificArticles([FromQuery] int page = 1)
        {
            try
            {
                if (page < 1)
                    page = 1;

                int offset = (page - 1) * 5;
                var mostPopularArticles = await _scientificArticleDoiTrackerService.GetTopWithMetadataAsync(offset, 5);

                return mostPopularArticles.ToList();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpPost("track")]
        public async Task<IActionResult> TrackView([FromBody] TrackRequest trackRequest)
        {
            try
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var a = new string(Enumerable.Repeat(chars, 3)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var isIpBlocked = await _ipAbuseService.IsBlockedAsync(a);
                _ = AddLinkToUserHistory(trackRequest.ArticleUrl, Request);
                if (isIpBlocked)
                {
                    _logger.LogInformation($"{ipAddress} tried to track view even tho blocked");
                }

                if (!UrlValidator.IsTrustedProviderUrl(trackRequest.ArticleUrl, TrustedProviders.AllowedHosts))
                {
                    _logger.LogInformation($"{ipAddress} tried to track ${trackRequest.ArticleUrl}");
                    _ = _ipAbuseService.RegisterInvalidAttemptAsync(a);

                    return Ok();
                }

                
                await _viewTracker.RegisterEventAsync(a, trackRequest.ArticleUrl);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return Ok();
            }
        }

        [HttpPost("track/scientific/download")]
        public async Task<IActionResult> TrackScientificDownload([FromBody] TrackRequest trackRequest)
        {
            try
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var a = new string(Enumerable.Repeat(chars, 3)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var isIpBlocked = await _ipAbuseService.IsBlockedAsync(a);
                   _ = AddLinkToUserHistory(trackRequest.ArticleUrl, Request);
                if (isIpBlocked)
                {
                    _logger.LogInformation($"{ipAddress} tried to track view even tho blocked");
                }

                if (!UrlValidator.IsTrustedProviderUrl(trackRequest.ArticleUrl, TrustedScientificDownloadProviders.AllowedHosts))
                {
                    _logger.LogInformation($"{ipAddress} tried to download ${trackRequest.ArticleUrl}");
                    _ = _ipAbuseService.RegisterInvalidAttemptAsync(a);

                    return Ok();
                }

             
                await _scientificArticleDownloadTrackerService.RegisterEventAsync(a, trackRequest.ArticleUrl);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return Ok();
            }
        }

        [HttpPost("track/scientific/doi")]
        public async Task<IActionResult> TrackScientificDoi([FromBody] TrackRequest trackRequest)
        {
            try
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var a = new string(Enumerable.Repeat(chars, 3)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var isIpBlocked = await _ipAbuseService.IsBlockedAsync(a);
                if (isIpBlocked)
                {
                    _logger.LogInformation($"{ipAddress} tried to track view even tho blocked");
                }

                if (!UrlValidator.IsTrustedProviderUrl(trackRequest.ArticleUrl, TrustedScientificDoIProviders.AllowedHosts))
                {
                    _logger.LogInformation($"{ipAddress} tried to doi ${trackRequest.ArticleUrl}");
                    _ = _ipAbuseService.RegisterInvalidAttemptAsync(a);

                    return Ok();
                }

                _ = AddLinkToUserHistory(trackRequest.ArticleUrl, Request);
                await _scientificArticleDoiTrackerService.RegisterEventAsync(a, trackRequest.ArticleUrl);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return Ok();
            }
        }

        private async Task AddLinkToUserHistory(string url, HttpRequest request)
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"];
                if (userIdentity == null) return;

                await _userRepository.AddHistoryLink(url, userIdentity.Id);
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
            }
        }


    }
}