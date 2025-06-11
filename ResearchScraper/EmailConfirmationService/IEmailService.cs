namespace ResearchScrapper.Api.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlContent);
    }
}