namespace ResearchScrapper.Api.Models
{
    public class SemanticScholarPaper
    {
        public required string Title { get; set; }
        public required string Abstract { get; set; }
        public required string Url { get; set; }
        public required string Venue { get; set; }
        public int Year { get; set; }
        public required string Doi { get; set; }
        public required List<SemanticScholarAuthor> Authors { get; set; }
    }

    public class SemanticScholarAuthor
    {
        public required string Name { get; set; }
    }

    public class SemanticScholarSearchResponse
    {
        public required List<SemanticScholarPaper> Data { get; set; }
    }
}
