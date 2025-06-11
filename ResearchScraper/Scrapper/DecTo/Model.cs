namespace ResearchScrapper.Api.Models
{
    public class DevToArticle
    {
        public required string Title { get; set; }
        public required string Url { get; set; }
        public required string Description { get; set; }
        public required DevToUser User { get; set; }
    }

    public class DevToUser
    {
        public required string Username { get; set; }
    }
}