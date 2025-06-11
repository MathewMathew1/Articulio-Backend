
namespace ResearchScrapper.Api.Service
{
    public class ArticleViewRecalculationService : IHostedService, IDisposable
    {
        private readonly ArticleViewTrackerService _trackerService;
        private readonly ScientificArticleDownloadTrackerService _scientificArticleDownloadTracker;
        private readonly ScientificArticleDoiTrackerService _scientificArticleDoiTrackerService;
        private readonly ILogger<ArticleViewRecalculationService> _logger;
        private Timer? _timer;

        public ArticleViewRecalculationService(ArticleViewTrackerService trackerService, ILogger<ArticleViewRecalculationService> logger,
        ScientificArticleDownloadTrackerService scientificArticleDownloadTrackerService, ScientificArticleDoiTrackerService scientificArticleDoiTrackerService)
        {
            _scientificArticleDownloadTracker = scientificArticleDownloadTrackerService;
            _trackerService = trackerService;
            _logger = logger;
            _scientificArticleDoiTrackerService = scientificArticleDoiTrackerService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting ArticleViewRecalculationService.");

            _timer = new Timer(async _ => await RecalculateAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private async Task RecalculateAsync()
        {
            try
            {
                IEnumerable<Task> tasks = [];
                tasks.Append( _trackerService.RecalculateAsync());
                tasks.Append(_scientificArticleDownloadTracker.RecalculateAsync());
                tasks.Append(_scientificArticleDoiTrackerService.RecalculateAsync());
                await Task.WhenAll(tasks);
                _logger.LogInformation("Article views recalculated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during article views recalculation.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping ArticleViewRecalculationService.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

