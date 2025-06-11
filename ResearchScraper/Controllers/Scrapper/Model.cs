using Microsoft.AspNetCore.Mvc;

namespace ResearchScrapper.Api.Models
{
    public class GetArticlesDto
    {
        [FromQuery]
        public required string Query { get; set; }
        [FromQuery]
        public required List<SourceType> Sources { get; set; }
    }
}