using HtmlAgilityPack;
using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public class MediumScraperService : IScraperService
    {
        public async Task<ArticleDtoResult> ScrapeAsync(string query, int pageSize, int pageCount, CancellationToken cancellationToken = default)
        {
            var tag = Uri.EscapeDataString(query.ToLowerInvariant().Replace(" ", "-"));
            var url = $"https://medium.com/tag/{tag}";

            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            var articleNodes = doc.DocumentNode.SelectNodes("//article[@data-testid='post-preview']");

            var results = new List<ArticleMetadata>();

            foreach (var article in articleNodes)
            {
                try
                {
                    var hrefNode = article.SelectSingleNode(".//div[@role='link' and @data-href]");
                    var href = hrefNode?.GetAttributeValue("data-href", null);

                    if (string.IsNullOrWhiteSpace(href))
                    {
                        var altHrefNode = article.SelectSingleNode(".//a[@rel='noopener follow']");
                        href = altHrefNode?.GetAttributeValue("href", null);
                        if (href != null && !href.StartsWith("http"))
                            href = "https://medium.com" + href;
                    }

                    var titleNode = article.SelectSingleNode(".//h2");
                    var excerptNode = article.SelectSingleNode(".//h3");
                    var authorNode = article.SelectSingleNode(".//a[@rel='noopener follow']//p");

                    if (!string.IsNullOrWhiteSpace(href) && titleNode != null)
                    {
                        results.Add(new ArticleMetadata
                        {
                            Url = href,
                            Title = titleNode.InnerText.Trim(),
                            Abstract = excerptNode?.InnerText.Trim(),
                            Author = authorNode?.InnerText.Trim(),
                            Source = "Medium"
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MediumScraper] Error parsing article: {ex.Message}");
                    continue;
                }
            }

            return new ArticleDtoResult
            {
                Articles = results,
                HasMore = false,
                NextPageOffset = pageCount + 1
            };
        }
    }

}

