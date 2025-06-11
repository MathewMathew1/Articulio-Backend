using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;
using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public class NewsApiScraperService : IScraperService
    {

        private readonly NewsApiClient _newsApiClient;

        public NewsApiScraperService(IConfiguration configuration)
        {
            var apiKey = configuration["NewsApi:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("NewsAPI key is missing in configuration.");
            _newsApiClient = new NewsApiClient(apiKey);


        }

        public async Task<ArticleDtoResult> ScrapeAsync(string query, int pageSize, int pageCount, CancellationToken cancellationToken = default)
        {
            var encodedQuery = Uri.EscapeDataString(query.ToLowerInvariant());
            var articlesResponse = _newsApiClient.GetEverything(new EverythingRequest
            {
                Q = query,
                SortBy = SortBys.Popularity,
                Language = Languages.EN,
                Page = pageCount,
                PageSize = pageSize
            });


            if (articlesResponse == null)
                return new ArticleDtoResult { Articles = Enumerable.Empty<ArticleMetadata>(), HasMore = false, NextPageOffset = pageCount + 1 };

            var articles = articlesResponse.Articles.Select(a => new ArticleMetadata
            {
                Title = a.Title,
                Url = a.Url,
                Source = a.Source?.Name ?? "NewsAPI",
                Author = a.Author ?? "Unknown",
                Abstract = a.Description ?? a.Title
            });

            bool hasMore = articles.Count() == pageSize;
            var fetchResults = new ArticleDtoResult { Articles = articles, HasMore = hasMore, NextPageOffset = pageCount + 1 };
            return fetchResults;
        }


    }
}
