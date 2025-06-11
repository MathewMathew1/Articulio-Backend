using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public interface IFavoriteArticleRepository
    {
        Task<FavoriteArticle> AddFavoriteAsync(CreateArticle article, string userId);
    
        Task<IEnumerable<FavoriteArticle>> GetFavoritesByUserIdAsync(string userId);
        Task<FavoriteArticle?> UpdateFavoriteAsync(CreateArticle article, string userId);
        Task<bool> DeleteFavoriteAsync(string id, string userId);
        Task<IReadOnlyList<FavoriteArticle>> GetAllByUserIdAsync(string userId);
    }

    public interface IToReadArticleRepository
    {
        Task<ToReadArticle> AddToReadAsync(CreateReadArticle article, string userId);
        Task<IEnumerable<ToReadArticle>> GetToReadsByUserIdAsync(string userId);
        Task<ToReadArticle?> UpdateToReadAsync(CreateReadArticle article, string userId);
        Task<bool> DeleteToReadAsync(string id, string userId);
        Task<IReadOnlyList<ToReadArticle>> GetAllByUserIdAsync(string userId);
    }
}
