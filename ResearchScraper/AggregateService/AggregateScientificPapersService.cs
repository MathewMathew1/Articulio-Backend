using ResearchScrapper.Api.Models;
using System.Collections.Concurrent;

namespace ResearchScrapper.Api.Service
{
    public class AggregateScientificPapersService: IAggregateScientificPapersService
    {
        private const int PAGE_SIZE = 20;
        private readonly CoreApiService _articleCoreService;
        private readonly ArxivArticleService _articleArxivArticleService;
        private readonly CrossrefScientificArticleService _crossrefScientificArticleService;
        private readonly PubMedArticleService _pubMedArticleService;
        private readonly ILogger<AggregateScientificPapersService> _logger;

        public AggregateScientificPapersService(
            CrossrefScientificArticleService crossrefScientificArticleService,
            CoreApiService articleCoreService, PubMedArticleService pubMedArticleService,
            ILogger<AggregateScientificPapersService> logger, ArxivArticleService arxivArticleService)
        {
            _crossrefScientificArticleService = crossrefScientificArticleService;
            _articleCoreService = articleCoreService;
            _logger = logger;
            _articleArxivArticleService = arxivArticleService;
            _pubMedArticleService = pubMedArticleService;
        }

        public async Task<Dictionary<SourceScientificArticleType, ArticleFetchResult>> ScrapeAsync(string query, IEnumerable<SourceScientificArticleType> sources, CancellationToken token, int pageCount
        , SearchTag searchTag)
        {
            var result = new ConcurrentDictionary<SourceScientificArticleType, ArticleFetchResult>();
            var tasks = sources.Select(source => Task.Run(async () =>
            {
                try
                {
                    ArticleFetchResult fetchResult = source switch
                    {
                        SourceScientificArticleType.Core => await _articleCoreService.SearchAsync(query, PAGE_SIZE, pageCount, searchTag, token),
                        SourceScientificArticleType.CrossRef => await _crossrefScientificArticleService.SearchAsync(query, PAGE_SIZE, pageCount, searchTag, token),
                        SourceScientificArticleType.ArXiv => await _articleArxivArticleService.SearchAsync(query, PAGE_SIZE, pageCount, searchTag, token),
                        SourceScientificArticleType.PubMed => await _pubMedArticleService.SearchAsync(query, PAGE_SIZE, pageCount, searchTag, token),
                        _ => new ArticleFetchResult{Articles = Enumerable.Empty<ScientificArticle>(),  HasMore =false, NextPageOffset=1}
                    };
                    result[source] = fetchResult;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting data from {source}");
                    result[source] = new ArticleFetchResult { Articles = Enumerable.Empty<ScientificArticle>(), HasMore = false, NextPageOffset = 1 };
                }
            }, token));

            await Task.WhenAll(tasks);
            return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}