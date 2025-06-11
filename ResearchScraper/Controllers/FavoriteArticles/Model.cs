using System.ComponentModel.DataAnnotations;

namespace ResearchScrapper.Api.Models
{


    public class CreateArticle
    {
        [Required]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public required string Title { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters.")]
        public required string Description { get; set; }

        [Required]
        [Url(ErrorMessage = "Invalid URL format.")]
        [StringLength(256, MinimumLength = 10, ErrorMessage = "URL must be a valid URI with reasonable length.")]
        public required string Url { get; set; }

        [StringLength(1000, MinimumLength = 10)]
        public required string? Notes { get; set; }
    }
}