using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public interface IArticleCacheService
    {
        Task<ArticleDtoResult> GetOrUpdateAsync(string query, Func<Task<ArticleDtoResult>> scrapeFunc, int pageSize, int pageCount, CancellationToken cancellationToken = default);
    }
}