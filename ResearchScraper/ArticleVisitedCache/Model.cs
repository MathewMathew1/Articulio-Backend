namespace ResearchScrapper.Api.Models
{

    public interface IArticleViewTrackerService
    {
        Task<bool> RegisterEventAsync(string ipAddress, string articleUrl);
        Task<int> GetCountAsync(string articleUrl);
        Task<IReadOnlyList<(string ArticleUrl, long Count)>> GetTopAsync(int skip, int take);
        Task<IReadOnlyList<MostPopularArticlesDto>> GetTopWithMetadataAsync(int skip, int take);
        Task RecalculateAsync();
    }
}