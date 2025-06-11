using ResearchScrapper.Api.Models;
using StackExchange.Redis;

namespace ResearchScrapper.Api.Service
{
    public class ScientificArticleDownloadTrackerService : BaseArticleMetricTrackerService, IArticleViewTrackerService
    {
        public ScientificArticleDownloadTrackerService(IConnectionMultiplexer redis, MetaScraper scraper)
            : base(redis.GetDatabase(), scraper) { }

        protected override string EventPrefix => "dlScientific";
        protected override string ObjectPrefix => "downloadedScientific";
        protected override string SortedSetKey => "dlScientific:sorted";
    }
}


