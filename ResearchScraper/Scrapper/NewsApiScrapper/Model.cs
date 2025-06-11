namespace ResearchScrapper.Api.Models
{
    public class NewsApiResponse
    {
        public required List<NewsArticle> Articles { get; set; }
    }

    public class NewsArticle
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Url { get; set; }
        public required string Author { get; set; }
        public required NewsSource Source { get; set; }
    }

    public class NewsSource
    {
        public required string Name { get; set; }
    }
}