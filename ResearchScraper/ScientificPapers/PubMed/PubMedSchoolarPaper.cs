using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;
using ResearchScrapper.Api.Models;
using System.Web;

namespace ResearchScrapper.Api.Service;

public class PubMedArticleService : IScientificArticleService
{
    private const string ESearchUrl = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi";
    private const string EFetchUrl = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi";
    private readonly HttpClient _httpClient;

    public PubMedArticleService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ArticleFetchResult> SearchAsync(string query, int pageSize, int pageCount, SearchTag tagSearch, CancellationToken cancellationToken = default)
    {
        var startIndex = (pageCount - 1) * pageSize;
        var searchQuery = BuildQuery(query, tagSearch);
        var ids = await FetchIdsAsync(searchQuery, pageSize, startIndex, cancellationToken);

        if (ids.Count == 0)
        {
            return new ArticleFetchResult();
        }

        var articles = await FetchArticlesAsync(ids, cancellationToken);

        return new ArticleFetchResult
        {
            Articles = articles,
            HasMore = ids.Count == pageSize,
            NextPageOffset = pageCount + 1
        };
    }

    private static string BuildQuery(string query, SearchTag tag)
    {
        return tag switch
        {
            SearchTag.Author => $"{query}[Author]",
            _ => query
        };
    }

    private async Task<List<string>> FetchIdsAsync(string query, int retmax, int retstart, CancellationToken cancellationToken)
    {
        var url = $"{ESearchUrl}?db=pubmed&retmode=json&retmax={retmax}&retstart={retstart}&term={HttpUtility.UrlEncode(query)}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return json.RootElement
            .GetProperty("esearchresult")
            .GetProperty("idlist")
            .EnumerateArray()
            .Select(e => e.GetString()!)
            .ToList();
    }

    private async Task<List<ScientificArticle>> FetchArticlesAsync(List<string> ids, CancellationToken cancellationToken)
    {
        var url = $"{EFetchUrl}?db=pubmed&retmode=xml&id={string.Join(",", ids)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var xDoc = XDocument.Parse(content);

        return xDoc.Descendants("PubmedArticle")
            .Select(ParseArticle)
            .ToList();
    }

    private ScientificArticle ParseArticle(XElement articleElement)
    {
        var medline = articleElement.Element("MedlineCitation");
        var article = medline?.Element("Article");

        var title = article?.Element("ArticleTitle")?.Value?.Trim() ?? "";
        var abstractText = article?.Element("Abstract")?.Element("AbstractText")?.Value?.Trim();
        var authors = article?.Element("AuthorList")?.Elements("Author")
            .Select(a =>
            {
                var last = a.Element("LastName")?.Value;
                var first = a.Element("ForeName")?.Value;
                return $"{first} {last}".Trim();
            })
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList() ?? [];

        var journal = article?.Element("Journal")?.Element("Title")?.Value;
        var year = article?.Element("Journal")?.Element("JournalIssue")?.Element("PubDate")?.Element("Year")?.Value;
        var doi = article?.Element("ELocationID")
            ?.Attributes()
            .FirstOrDefault(a => a.Value == "doi")?
            .Parent?.Value;

        var pmid = medline?.Element("PMID")?.Value;
        var url = pmid != null ? $"https://pubmed.ncbi.nlm.nih.gov/{pmid}/" : null;

        return new ScientificArticle
        {
            Title = title,
            Abstract = abstractText,
            Authors = authors,
            Doi = doi,
            Url = url,
            Journal = journal,
            Year = int.TryParse(year, out var y) ? y : null,
            Source = "PubMed"
        };
    }
}
