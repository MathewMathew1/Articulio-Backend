using MongoDB.Driver;
using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public class ToReadScientificArticleRepository : IToReadScientificArticleRepository
    {
        private readonly IMongoCollection<ToReadScientificArticle> _collection;

        public ToReadScientificArticleRepository(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("Cluster0");
            _collection = database.GetCollection<ToReadScientificArticle>("ToReadScientificArticles");

            var indexKeys = Builders<ToReadScientificArticle>.IndexKeys
                .Ascending(a => a.UserId)
                .Ascending(a => a.Url);

            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<ToReadScientificArticle>(indexKeys, indexOptions);

            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<ToReadScientificArticle> AddToReadAsync(CreateToReadScientificArticle article, string userId)
        {
            var fullArticle = new ToReadScientificArticle
            {
                Url = article.Url,
                UserId = userId,
                Title = article.Title,
                Description = article.Description,
                Doi = article.Doi,
                Download = article.Download
            };

            await _collection.InsertOneAsync(fullArticle);
            return fullArticle;
        }

        public async Task<IReadOnlyList<ToReadScientificArticle>> GetAllByUserIdAsync(string userId)
        {
            return await _collection.Find(a => a.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<ToReadScientificArticle>> GetToReadsByUserIdAsync(string userId)
        {
            return await _collection.Find(a => a.UserId == userId).ToListAsync();
        }

        public async Task<ToReadScientificArticle?> UpdateToReadAsync(string id, CreateToReadScientificArticle article, string userId)
        {
            var filter = Builders<ToReadScientificArticle>.Filter.Where(a => a.Id == id && a.UserId == userId);
            var update = Builders<ToReadScientificArticle>.Update
                .Set(a => a.Title, article.Title)
                .Set(a => a.Description, article.Description)
                .Set(a => a.Doi, article.Doi)
                .Set(a => a.Download, article.Download);

            var result = await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<ToReadScientificArticle>
                {
                    ReturnDocument = ReturnDocument.After
                });

            return result;
        }

        public async Task<bool> DeleteToReadAsync(string id, string userId)
        {
            var result = await _collection.DeleteOneAsync(a => a.Id == id && a.UserId == userId);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}
