namespace ResearchScrapper.Api.Models
{
    public class ArticleMetadata
    {
        public required string Title { get; set; }
        public required string Url { get; set; }
        public required string Source { get; set; }
        public  string? Author { get; set; }
        public DateTime? PublishedDate { get; set; }
        public  string? Abstract { get; set; }
    }

       public class ArticleDtoResult
    {
        public IEnumerable<ArticleMetadata> Articles { get; set; } = Enumerable.Empty<ArticleMetadata>();
        public bool HasMore { get; set; } = false;
        public int? NextPageOffset { get; set; }
    }
}