using MongoDB.Driver;
using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public class FavoriteScientificArticleRepository : IFavoriteScientificArticleRepository
    {
        private readonly IMongoCollection<SavedScientificArticle> _collection;

        public FavoriteScientificArticleRepository(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("Cluster0");
            _collection = database.GetCollection<SavedScientificArticle>("ScientificFavoriteArticles");

            var indexKeys = Builders<SavedScientificArticle>.IndexKeys
                .Ascending(a => a.UserId)
                .Ascending(a => a.Url);

            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<SavedScientificArticle>(indexKeys, indexOptions);

            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<SavedScientificArticle> AddFavoriteAsync(CreateFavoriteScientificArticle article, string userId)
        {
            var fullArticle = new SavedScientificArticle
            {
                Url = article.Url,
                UserId = userId,
                Description = article.Description,
                Title = article.Title,
                Notes = article.Notes,
                Doi = article.Doi,
                Download = article.Download
            };

            await _collection.InsertOneAsync(fullArticle);
            return fullArticle;
        }

        public async Task<IReadOnlyList<SavedScientificArticle>> GetAllByUserIdAsync(string userId)
        {
            return await _collection.Find(a => a.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<SavedScientificArticle>> GetFavoritesByUserIdAsync(string userId)
        {
            return await _collection.Find(a => a.UserId == userId).ToListAsync();
        }

        public async Task<SavedScientificArticle?> UpdateFavoriteAsync(string id, CreateFavoriteScientificArticle article, string userId)
        {
            var filter = Builders<SavedScientificArticle>.Filter.Where(a => a.Id == id && a.UserId == userId);
            var update = Builders<SavedScientificArticle>.Update
                .Set(a => a.Title, article.Title)
                .Set(a => a.Description, article.Description)
                .Set(a => a.Notes, article.Notes)
                .Set(a => a.Doi, article.Doi)
                .Set(a => a.Download, article.Download);

            var result = await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<SavedScientificArticle>
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
