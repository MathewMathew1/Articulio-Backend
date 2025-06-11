using ResearchScrapper.Api.Models;


namespace ResearchScrapper.Api.Service
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(CreateUser user);
        Task<User> CreateUserWithEmailPasswordAsync(CreateUserWithPassword user);
        Task AddHistoryLink(string visitedUrl, string userId);
    }
}
