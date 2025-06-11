namespace ResearchScrapper.Api.Models
{
    public class ScientificArticle
    {
        public required string Title { get; set; }
        public string? Abstract { get; set; }
        public List<string> Authors { get; set; } = [];
        public string? Doi { get; set; }
        public string? Url { get; set; }
        public string? Journal { get; set; }
        public int? Year { get; set; }
        public string? FullText { get; set; }
        public string? DownloadUrl { get; set; }
        public string Source { get; set; } = "CORE";
    }

    public class ArticleFetchResult
    {
        public IEnumerable<ScientificArticle> Articles { get; set; } = Enumerable.Empty<ScientificArticle>();
        public bool HasMore { get; set; } = false;
        public int? NextPageOffset { get; set; }
    }

    public enum SearchTag
    {
        Tag,
        Author,
    }
}