using MongoDB.Driver;
using ResearchScrapper.Api.Models;


namespace ResearchScrapper.Api.Service
{
    public class ToReadArticleRepository : IToReadArticleRepository
    {
        private readonly IMongoCollection<ToReadArticle> _collection;

        public ToReadArticleRepository(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("Cluster0");
            _collection = database.GetCollection<ToReadArticle>("Cluster0");

              _collection = database.GetCollection<ToReadArticle>("ToReadArticles");

            var indexKeys = Builders<ToReadArticle>.IndexKeys
                .Ascending(a => a.UserId)
                .Ascending(a => a.Url);

            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<ToReadArticle>(indexKeys, indexOptions);

            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<ToReadArticle> AddToReadAsync(CreateReadArticle article, string userId)
        {
             var fullArticle = new ToReadArticle
            {
                Url = article.Url,
                UserId = userId,
                Description = article.Description,
                Title = article.Title,
            };

            await _collection.InsertOneAsync(fullArticle);
            return fullArticle;
        }


        public async Task<IReadOnlyList<ToReadArticle>> GetAllByUserIdAsync(string userId)
        {
            return await _collection.Find(a => a.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<ToReadArticle>> GetToReadsByUserIdAsync(string userId)
        {
            return await _collection.Find(a => a.UserId == userId).ToListAsync();
        }

        public async Task<ToReadArticle?> UpdateToReadAsync(CreateReadArticle article, string userId)
        {
            var filter = Builders<ToReadArticle>.Filter.Where(a => a.UserId == userId && a.Url == article.Url);
            var update = Builders<ToReadArticle>.Update
                .Set(a => a.Title, article.Title)
                .Set(a => a.Description, article.Description);


            var result = await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<ToReadArticle>
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
