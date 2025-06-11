using System.Text.Json;
using System.Text.Json.Serialization;
using ResearchScrapper.Api.Models;
using System.Web;

namespace ResearchScrapper.Api.Service
{
    public sealed class CrossrefScientificArticleService : IScientificArticleService
    {
        private readonly HttpClient _httpClient;
        private const string CrossrefEndpoint = "https://api.crossref.org/works?query=";

        private readonly string _email;

        public CrossrefScientificArticleService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _email = config["email"] ?? "";

        }


        public async Task<ArticleFetchResult> SearchAsync(string query, int pageSize, int pageCount, SearchTag tagSearch, CancellationToken cancellationToken = default)
        {
            var encodedQuery = HttpUtility.UrlEncode(query);
            var url = $"{CrossrefEndpoint}{encodedQuery}&rows={pageSize}&offset={pageSize * (pageCount - 1)}mailto={_email}";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<CrossrefResponse>(
                contentStream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                },
                cancellationToken
            );
            var articles = result?.Message?.Items?.Select(item => new ScientificArticle
            {
                Title = item.Title?.FirstOrDefault() ?? "Untitled",
                Abstract = item.Abstract,
                Authors = item.Author?.Select(a => $"{a.Given} {a.Family}").ToList() ?? [],
                Doi = item.DOI,
                Url = item.URL,
                Journal = item.ContainerTitle?.FirstOrDefault(),
                Year = item.Issued?.DateParts?.FirstOrDefault()?.FirstOrDefault(),
                Source = "Crossref"
            });

            bool hasMore = articles.Count() == pageSize;
            var fetchResults = new ArticleFetchResult { Articles = articles, HasMore = hasMore, NextPageOffset = pageCount + 1 };
            return fetchResults;
        }
    }


}
