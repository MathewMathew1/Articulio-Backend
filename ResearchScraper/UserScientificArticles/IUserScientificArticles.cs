

using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public interface IFavoriteScientificArticleRepository
    {
        Task<SavedScientificArticle> AddFavoriteAsync(CreateFavoriteScientificArticle article, string userId);
        Task<IEnumerable<SavedScientificArticle>> GetFavoritesByUserIdAsync(string userId);
        Task<SavedScientificArticle?> UpdateFavoriteAsync(string id, CreateFavoriteScientificArticle article, string userId);
        Task<bool> DeleteFavoriteAsync(string id, string userId);
        Task<IReadOnlyList<SavedScientificArticle>> GetAllByUserIdAsync(string userId);
    }

    public interface IToReadScientificArticleRepository
    {
        Task<ToReadScientificArticle> AddToReadAsync(CreateToReadScientificArticle article, string userId);
        Task<IEnumerable<ToReadScientificArticle>> GetToReadsByUserIdAsync(string userId);
        Task<ToReadScientificArticle?> UpdateToReadAsync(string id, CreateToReadScientificArticle article, string userId);
        Task<bool> DeleteToReadAsync(string id, string userId);
        Task<IReadOnlyList<ToReadScientificArticle>> GetAllByUserIdAsync(string userId);
    }
}