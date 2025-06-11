using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public interface IScraperService
    {
        Task<ArticleDtoResult> ScrapeAsync(string query, int pageSize, int pageCount, CancellationToken cancellationToken = default);
    }
}