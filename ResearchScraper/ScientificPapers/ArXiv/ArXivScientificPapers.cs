using System.Xml.Linq;
using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public class ArxivArticleService : IScientificArticleService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://export.arxiv.org/api/query";

        public ArxivArticleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ArticleFetchResult> SearchAsync(string query, int pageSize, int pageCount, SearchTag searchTag, CancellationToken cancellationToken = default)
        {
            var start = (pageCount - 1) * pageSize;
            var searchType = searchTag == SearchTag.Author ? "au" : "all";
            var requestUrl = $"{BaseUrl}?search_query={searchType}:{query}&start={start}&max_results={pageSize}";

            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var xmlContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseAtomResponse(xmlContent, pageCount, pageSize);
        }

        private static (bool isAuthorSearch, string coreQuery) ParseQuery(string query)
        {
            if (query.StartsWith("authors:", StringComparison.OrdinalIgnoreCase))
            {
                return (true, query.Substring("authors:".Length).Trim('"'));
            }
            return (false, query);
        }

        private ArticleFetchResult ParseAtomResponse(string xml, int page, int pageSize)
        {
            var xdoc = XDocument.Parse(xml);
            XNamespace ns = "http://www.w3.org/2005/Atom";
            XNamespace opensearch = "http://a9.com/-/spec/opensearch/1.1/";

            var total = int.Parse(xdoc.Root?.Element(opensearch + "totalResults")?.Value ?? "0");

            var entries = xdoc.Descendants(ns + "entry").Select(entry =>
            {
                var title = entry.Element(ns + "title")?.Value?.Trim() ?? "title";
                var summary = entry.Element(ns + "summary")?.Value?.Trim();
                var published = DateTime.TryParse(entry.Element(ns + "published")?.Value, out var dt) ? dt : DateTime.MinValue;
                var link = entry.Elements(ns + "link")
                    .FirstOrDefault(l => l.Attribute("type")?.Value == "application/pdf")?
                    .Attribute("href")?.Value;

                var authors = entry.Elements(ns + "author")
                    .Select(a => a.Element(ns + "name")?.Value)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();

                return new ScientificArticle
                {
                    Title = title,
                    Abstract = summary,
                    Year = int.Parse(published.ToString("yyyy")),
                    Authors = authors,
                    DownloadUrl = link
                };
            }).ToList();

            bool hasMore = entries.Count() == pageSize;
            var fetchResults = new ArticleFetchResult { Articles = entries, HasMore = hasMore, NextPageOffset = page + 1 };
            return fetchResults;

        }
    }
}
