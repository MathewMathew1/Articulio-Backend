using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public interface IAggregateService
    {
        Task<Dictionary<SourceType, ArticleDtoResult>> ScrapeAsync(string query, IEnumerable<SourceType> sources, int pageCount, CancellationToken token);
    }
}