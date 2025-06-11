using MongoDB.Driver;
using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public class FavoriteArticleRepository : IFavoriteArticleRepository
    {
        private readonly IMongoCollection<FavoriteArticle> _collection;

        public FavoriteArticleRepository(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("Cluster0");
            _collection = database.GetCollection<FavoriteArticle>("FavoriteArticles");

            var indexKeys = Builders<FavoriteArticle>.IndexKeys.Ascending(a => a.Url);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<FavoriteArticle>(indexKeys, indexOptions);

            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<FavoriteArticle> AddFavoriteAsync(CreateArticle article, string userId)
        {
            var fullArticle = new FavoriteArticle
            {
                Url = article.Url,
                UserId = userId,
                Description = article.Description,
                Title = article.Title,
                Notes = article.Notes,
            };

            await _collection.InsertOneAsync(fullArticle);
            return fullArticle;
        }




        public async Task<IReadOnlyList<FavoriteArticle>> GetAllByUserIdAsync(string userId)
        {
            var favoritesArticles = await _collection.Find(a => a.UserId == userId).ToListAsync();

            return favoritesArticles;
        }

        public async Task<IEnumerable<FavoriteArticle>> GetFavoritesByUserIdAsync(string userId)
        {
            return await _collection.Find(a => a.UserId == userId).ToListAsync();
        }

        public async Task<FavoriteArticle?> UpdateFavoriteAsync(CreateArticle article, string userId)
        {
            var filter = Builders<FavoriteArticle>.Filter.Where(a => a.UserId == userId && a.Url == article.Url);
            var update = Builders<FavoriteArticle>.Update
                .Set(a => a.Title, article.Title)
                .Set(a => a.Description, article.Description)
                .Set(a => a.Notes, article.Notes);

            var result = await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<FavoriteArticle>
                {
                    ReturnDocument = ReturnDocument.After
                });

            return result;
        }


        public async Task<bool> DeleteFavoriteAsync(string id, string userId)
        {
            var result = await _collection.DeleteOneAsync(a => a.Id == id && a.UserId == userId);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}
