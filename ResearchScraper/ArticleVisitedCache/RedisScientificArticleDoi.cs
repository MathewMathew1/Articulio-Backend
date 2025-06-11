using ResearchScrapper.Api.Models;
using StackExchange.Redis;

namespace ResearchScrapper.Api.Service
{
    public class ScientificArticleDoiTrackerService : BaseArticleMetricTrackerService, IArticleViewTrackerService
    {
        public ScientificArticleDoiTrackerService(IConnectionMultiplexer redis, MetaScraper scraper)
            : base(redis.GetDatabase(), scraper) { }

        protected override string EventPrefix => "doiScientific";
        protected override string ObjectPrefix => "doiScientificArticle";
        protected override string SortedSetKey => "doiScientific:sorted";
    }
}


