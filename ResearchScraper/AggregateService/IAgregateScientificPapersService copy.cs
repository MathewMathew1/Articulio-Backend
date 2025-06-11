using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public interface IAggregateScientificPapersService
    {
        Task<Dictionary<SourceScientificArticleType, ArticleFetchResult>> ScrapeAsync(
            string query,
            IEnumerable<SourceScientificArticleType> sources,
            CancellationToken cancellationToken,
            int page,
            SearchTag tag);
    }
}