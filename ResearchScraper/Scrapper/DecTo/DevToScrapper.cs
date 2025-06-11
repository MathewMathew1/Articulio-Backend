using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public class DevToScraperService : IScraperService
    {
        private readonly HttpClient _http;

        public DevToScraperService(HttpClient httpClient, IConfiguration configuration)
        {
            _http = httpClient;

            var apiKey = configuration["DevTo:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Dev.to API key is missing in configuration.");

            _http.DefaultRequestHeaders.Add("Accept", "application/vnd.forem.api-v1+json");
            _http.DefaultRequestHeaders.Add("api-key", apiKey);
        }

        public async Task<ArticleDtoResult> ScrapeAsync(string query, int pageSize, int pageCount, CancellationToken cancellationToken = default)
        {
            var tag = Uri.EscapeDataString(query.ToLowerInvariant());
            var url = $"https://dev.to/api/articles?tag={tag}&per_page={pageSize}&page={pageCount}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/vnd.forem.api-v1+json");
            request.Headers.Add("User-Agent", "ResearchScrapper/1.0");

            var response = await _http.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed with status code {response.StatusCode}");

            var articles = await response.Content.ReadFromJsonAsync<List<DevToArticle>>(cancellationToken: cancellationToken);
            var articlesWithMetaData = articles?.Select(a => new ArticleMetadata
            {
                Title = a.Title,
                Url = a.Url,
                Source = "Dev.to",
                Author = a.User.Username,
                Abstract = a.Description
            }) ?? Enumerable.Empty<ArticleMetadata>();


            bool hasMore = articlesWithMetaData.Count() == pageSize; 
            var fetchResults = new ArticleDtoResult { Articles = articlesWithMetaData, HasMore = hasMore, NextPageOffset = pageCount + 1 }; 
            return fetchResults;
        }


    }
}
