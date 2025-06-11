using System.ComponentModel.DataAnnotations;

namespace ResearchScrapper.Api.Models
{

    public class TrackRequest
    {
        [Required]
        [Url(ErrorMessage = "Invalid URL format.")]
        [StringLength(2048, MinimumLength = 10, ErrorMessage = "Article URL must be a valid URI between 10 and 2048 characters.")]
        public required string ArticleUrl { get; set; }
    }

    public class MostPopularArticlesDto
    {
        public required string ArticleUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
