using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResearchScrapper.Api.Models
{
    public class SavedArticle
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

        [BsonElement("userId")]
        public required string UserId { get; set; }
    }

    public class FavoriteArticle
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Url { get; set; }
        public required string UserId { get; set; }
        public required string? Notes { get; set; }
    }

    public class ToReadArticle
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Url { get; set; }
        public required string UserId { get; set; }
    }


    public class CreateSavedArticle
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Url { get; set; }
    }
}
