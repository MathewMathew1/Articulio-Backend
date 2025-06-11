namespace ResearchScrapper.Api.Models
{

    public interface IQuerySanitizationService
    {
        string Sanitize(string input);
        bool IsValid(string input);
    }
}