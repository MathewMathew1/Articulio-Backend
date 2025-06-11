using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResearchScrapper.Api.Models
{
    public class SavedScientificArticle
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("title")]
        public required string Title { get; set; }

        [BsonElement("description")]
        public required string Description { get; set; }

        [BsonElement("url")]
        public required string Url { get; set; }

        [BsonElement("doi")]
        public string? Doi { get; set; }

        [BsonElement("download")]
        public string? Download { get; set; }

        public required string? Notes { get; set; }
        [BsonElement("userId")]
        public required string UserId { get; set; }
    }

    public class ToReadScientificArticle
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public required string Title { get; set; }

        [BsonElement("description")]
        public required string Description { get; set; }

        [BsonElement("url")]
        public required string Url { get; set; }

        [BsonElement("doi")]
        public string? Doi { get; set; }

        [BsonElement("download")]
        public string? Download { get; set; }

        [BsonElement("userId")]
        public required string UserId { get; set; }
    }

    public class CreateFavoriteScientificArticle
    {
        [Required]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public required string Title { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters.")]
        public required string Description { get; set; }

        [Required]
        [Url(ErrorMessage = "Invalid URL format.")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "URL must be a valid URI with reasonable length.")]
        public required string Url { get; set; }

        [StringLength(100, ErrorMessage = "DOI must be under 100 characters.")]
        public string? Doi { get; set; }

        [Url(ErrorMessage = "Invalid download URL format.")]
        [StringLength(200, ErrorMessage = "Download URL must be under 200 characters.")]
        public string? Download { get; set; }

        [StringLength(2000, ErrorMessage = "Notes must be under 2000 characters.")]
        public string? Notes { get; set; }
    }

    public class CreateToReadScientificArticle
    {
        [Required]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public required string Title { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters.")]
        public required string Description { get; set; }

        [Required]
        [Url(ErrorMessage = "Invalid URL format.")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "URL must be a valid URI with reasonable length.")]
        public required string Url { get; set; }

        [StringLength(100, ErrorMessage = "DOI must be under 100 characters.")]
        public string? Doi { get; set; }

        [Url(ErrorMessage = "Invalid download URL format.")]
        [StringLength(200, ErrorMessage = "Download URL must be under 200 characters.")]
        public string? Download { get; set; }
    }

}
