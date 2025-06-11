using ResearchScrapper.Api.Models;
using StackExchange.Redis;

namespace ResearchScrapper.Api.Service
{
    public abstract class BaseArticleMetricTrackerService
    {
        protected readonly IDatabase _db;
        protected readonly MetaScraper _metaScraper;

        protected abstract string EventPrefix { get; }
        protected abstract string ObjectPrefix { get; }
        protected abstract string SortedSetKey { get; }

        protected BaseArticleMetricTrackerService(IDatabase db, MetaScraper metaScraper)
        {
            _db = db;
            _metaScraper = metaScraper;
        }

        public async Task<bool> RegisterEventAsync(string ipAddress, string articleUrl)
        {
            var ipKey = $"{EventPrefix}:{articleUrl}:{ipAddress}";
            return await _db.StringSetAsync(ipKey, "1", TimeSpan.FromDays(3), When.NotExists);
        }

        public async Task<int> GetCountAsync(string articleUrl)
        {
            var score = await _db.SortedSetScoreAsync(SortedSetKey, articleUrl);
            return score.HasValue ? (int)score.Value : 0;
        }

        public async Task<IReadOnlyList<(string ArticleUrl, long Count)>> GetTopAsync(int skip, int take)
        {
            var results = await _db.SortedSetRangeByRankWithScoresAsync(
                key: SortedSetKey,
                start: skip,
                stop: skip + take - 1,
                order: Order.Descending
            );

            return results.Select(e => (e.Element.ToString(), (long)e.Score)).ToList();
        }

        public async Task<IReadOnlyList<MostPopularArticlesDto>> GetTopWithMetadataAsync(int skip, int take)
        {
            var top = await GetTopAsync(skip, take);
            var results = new List<MostPopularArticlesDto>();

            foreach (var (url, _) in top)
            {
                var metaKey = $"{ObjectPrefix}:meta:{url}";
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

        public async Task RecalculateAsync()
        {
            var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{EventPrefix}:*").ToArray();

            var grouped = keys
                .Select(key => key.ToString().Split(':'))
                .Where(parts => parts.Length >= 3)
                .GroupBy(parts => parts[1] + ":" + parts[2]); 

            await _db.KeyDeleteAsync(SortedSetKey);

            foreach (var group in grouped)
            {
                var articleUrl = group.Key;
                var uniqueIps = group.Count();

                if (uniqueIps > 0)
                    await _db.SortedSetAddAsync(SortedSetKey, articleUrl, uniqueIps);

                await CacheMetadataIfMissingAsync(articleUrl);
            }
        }

        private async Task CacheMetadataIfMissingAsync(string articleUrl)
        {
            var metaKey = $"{ObjectPrefix}:meta:{articleUrl}";
            var title = await _db.HashGetAsync(metaKey, "title");

            if (title.IsNullOrEmpty)
            {
                var metadata = _metaScraper.GetMetaDataFromUrl(articleUrl);
                var hashEntries = new List<HashEntry>();

                if (!string.IsNullOrEmpty(metadata.Title))
                    hashEntries.Add(new HashEntry("title", metadata.Title));
                if (!string.IsNullOrEmpty(metadata.Description))
                    hashEntries.Add(new HashEntry("description", metadata.Description));

                if (hashEntries.Any())
                {
                    await _db.HashSetAsync(metaKey, hashEntries.ToArray());
                    await _db.KeyExpireAsync(metaKey, TimeSpan.FromHours(12));
                }
            }
        }
    }
}
