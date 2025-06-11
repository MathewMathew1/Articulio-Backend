namespace ResearchScrapper.Api.Models
{

    public interface IIpAbuseService
    {
        Task<bool> IsBlockedAsync(string ip);
        Task RegisterInvalidAttemptAsync(string ip);
    }
}