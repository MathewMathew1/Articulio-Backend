using System.Text.RegularExpressions;
using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service
{

    public sealed class QuerySanitizationService : IQuerySanitizationService
    {
        private const int MaxLength = 200;

        private static readonly Regex AllowedPattern = new(@"^[\w\s\-:,""'()\[\].]+$", RegexOptions.Compiled);

        public string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var trimmed = input.Trim();

            if (trimmed.Length > MaxLength)
                trimmed = trimmed.Substring(0, MaxLength);

            if (!AllowedPattern.IsMatch(trimmed))
                throw new ArgumentException("Query contains invalid characters.");

            return trimmed;
        }

        public bool IsValid(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length > MaxLength)
                return false;

            return AllowedPattern.IsMatch(input);
        }
    }
}
