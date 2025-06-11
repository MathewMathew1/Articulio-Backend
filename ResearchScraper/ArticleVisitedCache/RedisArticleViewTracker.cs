using ResearchScrapper.Api.Models;
using StackExchange.Redis;

namespace ResearchScrapper.Api.Service
{
    public class ArticleViewTrackerService : BaseArticleMetricTrackerService, IArticleViewTrackerService
    {
        public ArticleViewTrackerService(IConnectionMultiplexer redis, MetaScraper scraper)
            : base(redis.GetDatabase(), scraper) { }

        protected override string ObjectPrefix => "article";
        protected override string EventPrefix => "viewed";
        protected override string SortedSetKey => "views:sorted";
    }
}

/*namespace ResearchScrapper.Api.Service
{
    public class ArticleViewTrackerService : IArticleViewTrackerService
    {
        private readonly IDatabase _db;
        private readonly TimeSpan _ipEntryTtl = TimeSpan.FromDays(3);
        private const string SortedSetKey = "views:sorted";
        private readonly MetaScraper _metaScraper;

        public ArticleViewTrackerService(IConnectionMultiplexer redis, MetaScraper metaScraper)
        {
            _db = redis.GetDatabase();
            _metaScraper = metaScraper;
        }

        public async Task<bool> RegisterViewAsync(string ipAddress, string articleUrl)
        {
            var ipKey = $"viewed:{articleUrl}:{ipAddress}";

            var added = await _db.StringSetAsync(ipKey, "1", _ipEntryTtl, When.NotExists);


            return added;
        }

        public async Task<int> GetViewCountAsync(string articleUrl)
        {
            var score = await _db.SortedSetScoreAsync(SortedSetKey, articleUrl);
            return score.HasValue ? (int)score.Value : 0;
        }

        public async Task<IReadOnlyList<MostPopularArticlesDto>> GetTopViewedArticlesWithMetadataAsync(int skip, int take)
        {
            var top = await GetTopViewedArticlesAsync(skip, take);

            var results = new List<MostPopularArticlesDto>();

            foreach (var (url, count) in top)
            {
                var metaKey = $"article:meta:{url}";
                var meta = await _db.HashGetAllAsync(metaKey);

                string? title = meta.FirstOrDefault(e => e.Name == "title").Value;
                string? description = meta.FirstOrDefault(e => e.Name == "description").Value;

                results.Add(new MostPopularArticlesDto
                {
                    ArticleUrl = url,
                    Title = title,
                    Description = description
                });
            }

            return results;
        }


        public async Task<IReadOnlyList<(string ArticleUrl, long ViewCount)>> GetTopViewedArticlesAsync(int skip, int take)
        {
            if (skip < 0 || take <= 0)
                throw new ArgumentOutOfRangeException("Skip must be >= 0 and take > 0.");

            var results = await _db.SortedSetRangeByRankWithScoresAsync(
                key: SortedSetKey,
                start: skip,
                stop: skip + take - 1,
                order: Order.Descending
            );

            return results
                .Select(entry => (entry.Element.ToString(), (long)entry.Score))
                .ToList();
        }

        public async Task RecalculateArticleViewsAsync()
        {
            var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints().First());

            var keys = server.Keys(pattern: "viewed:*").ToArray();

            var grouped = keys
                .Select(key => key.ToString().Split(':'))
                .Where(parts => parts.Length == 4 && parts[0] == "viewed")
                .GroupBy(parts => parts[1] + ":" + parts[2]);

            await _db.KeyDeleteAsync(SortedSetKey);

            foreach (var group in grouped)
            {
                var articleUrl = group.Key;
                var uniqueIps = group.Count();

                if (uniqueIps > 0)
                {
                    await _db.SortedSetAddAsync(SortedSetKey, articleUrl, uniqueIps);
                }

                await CacheMetadataIfMissingAsync(articleUrl);
            }
        }

        private async Task CacheMetadataIfMissingAsync(string articleUrl)
        {
            var metaKey = $"article:meta:{articleUrl}";

            var title = await _db.HashGetAsync(metaKey, "title");

            if (title.IsNullOrEmpty)
            {
                var metadata = _metaScraper.GetMetaDataFromUrl(articleUrl);

                if (!string.IsNullOrEmpty(metadata.Title) || !string.IsNullOrEmpty(metadata.Description))
                {
                    var hashEntries = new List<HashEntry>();
                    if (!string.IsNullOrEmpty(metadata.Title))
                        hashEntries.Add(new HashEntry("title", metadata.Title));
                    if (!string.IsNullOrEmpty(metadata.Description))
                        hashEntries.Add(new HashEntry("description", metadata.Description));

                    await _db.HashSetAsync(metaKey, hashEntries.ToArray());
                    await _db.KeyExpireAsync(metaKey, TimeSpan.FromHours(12));
                }
            }
        }




    }
}*/
