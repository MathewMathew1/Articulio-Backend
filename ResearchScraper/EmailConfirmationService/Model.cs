namespace ResearchScrapper.Api.Models
{
    public class EmailConfirmationToken
    {
        public required string UserId { get; set; }
        public required string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}