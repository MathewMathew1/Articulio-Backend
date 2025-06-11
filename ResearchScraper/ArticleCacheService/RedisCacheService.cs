using System.Text.Json;
using ResearchScrapper.Api.Models;
using StackExchange.Redis;

namespace ResearchScrapper.Api.Service
{
    public class RedisBackedCacheService : IArticleCacheService
    {
        private readonly IDatabase _db;
        private readonly TimeSpan _expiration = TimeSpan.FromMinutes(10);

        public RedisBackedCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<ArticleDtoResult> GetOrUpdateAsync(string query, Func<Task<ArticleDtoResult>> scrapeFunc, int pageSize, int pageCount, CancellationToken cancellationToken = default)
        {
            var key = $"querycache:{query.ToLowerInvariant().Replace(" ", "_")}"+pageCount;

            var cached = await _db.StringGetAsync(key);
            if (cached.HasValue)
            {
                var data = JsonSerializer.Deserialize<IEnumerable<ArticleMetadata>>(cached);
                bool hasMore = data.Count() == pageSize;
                var fetchResults = new ArticleDtoResult { Articles = data, HasMore = false, NextPageOffset = pageCount + 1 };
                return fetchResults;
       
            }

            var scraped = await scrapeFunc();
            var json = JsonSerializer.Serialize(scraped.Articles);
            await _db.StringSetAsync(key, json, _expiration);

            return scraped;
        }
    }
}
