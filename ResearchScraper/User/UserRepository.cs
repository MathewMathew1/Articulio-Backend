using MongoDB.Driver;
using ResearchScrapper.Api.Models;
using BCrypt.Net;

namespace ResearchScrapper.Api.Service
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoClient mongoClient)
        {
            var db = mongoClient.GetDatabase("Cluster0");
            _users = db.GetCollection<User>("Users");


            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<User>(indexKeys, indexOptions);

            _users.Indexes.CreateOne(indexModel);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User> CreateUserAsync(CreateUser user)
        {
            var newUser = new User
            {
                Email = user.Email,
                Name = user.Name,
                PasswordHash = null,
                ConfirmedEmail = true,
                ProfileImageUrl = user.ProfileImageUrl
            };

            await _users.InsertOneAsync(newUser);
            return newUser;
        }

        public async Task AddHistoryLink(string visitedUrl, string userId)
        {
            await _users.UpdateOneAsync(
    u => u.Id == userId,
    Builders<User>.Update.PushEach(u => u.VisitedLinks,
        new[] { new VisitedLink { Url = visitedUrl } },
        slice: -100)
);
        }

        public async Task<User> CreateUserWithEmailPasswordAsync(CreateUserWithPassword user)
        {

            var existing = await GetUserByEmailAsync(user.Email);
            if (existing != null)
                throw new InvalidOperationException("User with this email already exists.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

            var newUser = new User
            {
                Email = user.Email,
                Name = user.Name,
                PasswordHash = passwordHash,
                ConfirmedEmail = false
            };

            await _users.InsertOneAsync(newUser);
            return newUser;
        }
    }
}
