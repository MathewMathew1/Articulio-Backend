using Microsoft.AspNetCore.Mvc;

namespace ResearchScrapper.Api.Models
{
    public class GetScientificArticlesDto
    {
        [FromQuery]
        public required string Query { get; set; }
        [FromQuery]
        public required List<SourceScientificArticleType> Sources { get; set; }
    }
}