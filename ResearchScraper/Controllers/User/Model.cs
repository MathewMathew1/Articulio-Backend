using System.ComponentModel.DataAnnotations;

namespace ResearchScrapper.Api.Models
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }


        [Required]
        [StringLength(128, MinimumLength = 8)]
        public required string Password { get; set; }
    }

    public class UserInfo
    {
        public required UserDto UserMainInfo { get; set; }
        public required IReadOnlyList<FavoriteArticle> FavoriteArticles { get; set; }
        public required IReadOnlyList<ToReadArticle> ToReadArticles { get; set; }
        public required IReadOnlyList<ToReadScientificArticle> ToReadScientificArticles { get; set; }
        public required IReadOnlyList<SavedScientificArticle> FavoriteScientificArticles { get; set; }
    }
}