namespace ResearchScrapper.Api.Models
{
    public record UserIdentity(string Id, string Email, string Name);

    public class JwtOptions
    {
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpiresMinutes { get; set; }
    }

    public class JwtPayloadPrincipal
    {
        public string? sub { get; set; }
        public string? email { get; set; }
        public string? name { get; set; }
        public long exp { get; set; }

        public string Id => sub ?? string.Empty;
        public string Email => email ?? string.Empty;
        public string Name => name ?? string.Empty;
    }
}