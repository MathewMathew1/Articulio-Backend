using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;



namespace ResearchScrapper.Api.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("email")]
        public required string Email { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("passwordHash")]
        public string? PasswordHash { get; set; }

        [BsonElement("profileImageUrl")]
        public string? ProfileImageUrl { get; set; }

        [BsonElement("confirmedEmail")]
        public bool ConfirmedEmail { get; set; }

        [BsonElement("visitedLinks")]
        public List<VisitedLink> VisitedLinks { get; set; } = new();
    }

    public class CreateUser
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public string? ProfileImageUrl { get; set; }

    }

    public class CreateUserWithPassword
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public class UserDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public string? ProfileImageUrl { get; set; }
        public required List<VisitedLink> VisitedLinks { get; set; }
    }

    public class VisitedLink
    {
        [BsonElement("url")]
        public required string Url { get; set; }

        

        [BsonElement("timestamp")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

