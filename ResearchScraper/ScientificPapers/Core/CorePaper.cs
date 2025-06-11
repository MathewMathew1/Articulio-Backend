using System.Net.Http.Headers;
using System.Text.Json;

using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public class CoreApiService : IScientificArticleService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoreApiService> _logger;
        private const string CoreApiBaseUrl = "https://api.core.ac.uk/v3/search/works";
        private readonly string _apiKey;

        public CoreApiService(HttpClient httpClient, ILogger<CoreApiService> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = config["CoreApiKey"] ?? throw new ArgumentNullException("CoreApiKey not configured");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }



        public async Task<ArticleFetchResult> SearchAsync(string query, int pageSize, int pageCount, SearchTag tagSearch , CancellationToken cancellationToken)
        {
            query = tagSearch == SearchTag.Author? $"authors:\"{query}\"": query;
            var url = $"{CoreApiBaseUrl}?q={Uri.EscapeDataString(query)}&offset={(pageCount-1)*pageSize}&limit={pageSize}";

            try
            {
                using var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                var articles = new List<ScientificArticle>();

                if (doc.RootElement.TryGetProperty("results", out var results))
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        var metadata = new ScientificArticle
                        {
                            Title = item.GetProperty("title").GetString() ?? string.Empty,
                            Abstract = item.TryGetProperty("abstract", out var abs) ? abs.GetString() : null,
                            Url = item.TryGetProperty("doi", out var doiProp) && doiProp.ValueKind == JsonValueKind.String
        ? $"https://doi.org/{doiProp.GetString()}"
        : item.TryGetProperty("url", out var urlProp) ? urlProp.GetString() : null,
                            Doi = item.TryGetProperty("doi", out var doiVal) ? doiVal.GetString() : null,
                            Journal = item.TryGetProperty("journal", out var journalProp) ? journalProp.GetString() : null,
                            Year = item.TryGetProperty("publishedDate", out var dateProp) &&
           DateTime.TryParse(dateProp.GetString(), out var date)
        ? date.Year
        : null,
                            Authors = item.TryGetProperty("authors", out var authorsProp) && authorsProp.ValueKind == JsonValueKind.Array
        ? authorsProp.EnumerateArray()
            .Select(a => a.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null)
            .Where(n => !string.IsNullOrEmpty(n))
            .Cast<string>()
            .ToList()
        : new List<string>(),
                            FullText = item.TryGetProperty("fullText", out var fullTextProp) ? fullTextProp.GetString() : null,
                            DownloadUrl = item.TryGetProperty("downloadUrl", out var downloadUrlProp) ? downloadUrlProp.GetString() : null,
                            Source = "CORE"
                        };



                        if (!string.IsNullOrEmpty(metadata.Title))
                            articles.Add(metadata);
                    }
                }

                bool hasMore = articles.Count() == pageSize;
                var fetchResults = new ArticleFetchResult { Articles = articles, HasMore = hasMore, NextPageOffset = pageCount + 1 }; 
                return fetchResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch articles from CORE API.");
                var fetchResults = new ArticleFetchResult { Articles = Enumerable.Empty<ScientificArticle>(), HasMore = false, NextPageOffset = pageCount + 1 }; 
                return fetchResults;
            }
        }
    }
}
