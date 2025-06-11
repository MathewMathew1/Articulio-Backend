using Microsoft.Playwright;
using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{
    public class MediumScraperService : IScraperService
    {
        public async Task<ArticleDtoResult> ScrapeAsync(string query, int pageSize, int pageCount, CancellationToken cancellationToken = default)
        {
            var tag = Uri.EscapeDataString(query.ToLowerInvariant().Replace(" ", "-"));
            var url = $"https://medium.com/tag/{tag}";

            var playwright = await Playwright.CreateAsync();
         var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = true
});
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/115.0.0.0 Safari/537.36"
            });

            var page = await context.NewPageAsync();
            await page.GotoAsync(url);
            await page.SetViewportSizeAsync(1280, 800);

            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            var articles = await page.Locator("article[data-testid='post-preview']").AllAsync();



            var results = new List<ArticleMetadata>();

            var tasks = articles.Take(10).Select(async article =>
            {
                try
                {
                    var titleTask = article.Locator("h2").First.InnerTextAsync(new LocatorInnerTextOptions { Timeout = 1000 });
                    var hrefTask = article.GetAttributeAsync("data-href");
                    var fallbackHrefTask = article.Locator("a").First.GetAttributeAsync("href");
                    var authorTask = article.Locator("a[rel='noopener follow'] >> p").First.InnerTextAsync(new LocatorInnerTextOptions { Timeout = 1000 });
                    var excerptTask = article.Locator("h3").First.InnerTextAsync(new LocatorInnerTextOptions { Timeout = 1000 });

                    await Task.WhenAll(titleTask, hrefTask, fallbackHrefTask, authorTask, excerptTask);

                    var title = titleTask.Result;
                    var href = hrefTask.Result ?? fallbackHrefTask.Result;
                    var author = authorTask.Result;
                    var excerpt = excerptTask.Result;

                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(href))
                    {
                        lock (results)
                        {
                            results.Add(new ArticleMetadata
                            {
                                Title = title.Trim(),
                                Url = href.StartsWith("http") ? href : "https://medium.com" + href,
                                Author = author?.Trim(),
                                Abstract = excerpt?.Trim(),
                                Source = "Medium"
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MediumScraper] Failed to parse article: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
            _ = browser.CloseAsync();
            
            bool hasMore = articles.Count() == pageSize; 
            var fetchResults = new ArticleDtoResult { Articles = results, HasMore = false, NextPageOffset = pageCount + 1 }; 
            return fetchResults;
        }
    }
}

