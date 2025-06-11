using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public interface IScientificArticleService
    {
        Task<ArticleFetchResult> SearchAsync(string query, int pageSize, int pageCount, SearchTag tagSearch, CancellationToken cancellationToken = default);
    }
}