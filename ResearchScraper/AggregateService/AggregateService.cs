using ResearchScrapper.Api.Models;
using System.Collections.Concurrent;

namespace ResearchScrapper.Api.Service
{
    public class AggregateScraperService: IAggregateService
    {
        private readonly MediumScraperService _medium;
        private readonly DevToScraperService _devTo;
        private readonly NewsApiScraperService _news;
        private readonly ILogger<AggregateScraperService> _logger;
        private readonly IArticleCacheService _cache;

        public AggregateScraperService(
            MediumScraperService medium,
            DevToScraperService devTo,
            NewsApiScraperService news,
             IArticleCacheService cache,
            ILogger<AggregateScraperService> logger)
        {
            _medium = medium;
            _devTo = devTo;
            _news = news;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Dictionary<SourceType, ArticleDtoResult>> ScrapeAsync(string query, IEnumerable<SourceType> sources, int pageCount, CancellationToken token )
        {
            var result = new ConcurrentDictionary<SourceType, ArticleDtoResult>();
            var tasks = sources.Select(source => Task.Run(async () =>
            {
                try
                {
                    ArticleDtoResult articles = source switch
                    {
                        SourceType.Medium => await _cache.GetOrUpdateAsync(query, () => _medium.ScrapeAsync(query, 10, pageCount, token), 10, pageCount, token),
                        SourceType.DevTo => await _devTo.ScrapeAsync(query, 15, pageCount, token),
                        SourceType.News => await _news.ScrapeAsync(query, 100, pageCount, token),
                        _ => new ArticleDtoResult{Articles = Enumerable.Empty<ArticleMetadata>(),  HasMore =false, NextPageOffset=1}
                    };
                    result[source] = articles;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error scraping {source}");
                    result[source] = new ArticleDtoResult { Articles = Enumerable.Empty<ArticleMetadata>(), HasMore = false, NextPageOffset = 1 };
                }
            }, token));

            await Task.WhenAll(tasks);
            return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
