namespace ResearchScrapper.Api.Models
{
    public static class TrustedProviders
    {
        public static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "medium.com",
            "dev.to",
            "github.com",
            "arxiv.org",
            "nytimes.com",
            "bbc.com",
            "theguardian.com",
            "wired.com",
            "arstechnica.com",
            "techcrunch.com",
            "theverge.com",
            "bloomberg.com",
            "reuters.com",
            "washingtonpost.com",
            "https://gizmodo.com",
            "forbes.com",
            "cnn.com",
            "nbcnews.com",
            "cnbc.com",
            "businessinsider.com",
            "hbr.org",
            "npr.org",
            "scientificamerican.com",
            "nature.com",
            "sciencedaily.com",
            "newscientist.com",
            "slashdot.org",
            "hackernoon.com",
            "dpreview.com",
            "fly.io",
            "simonwillison.net",
            "xuanwo.io",
            "news.ycombinator.com"
        };
    }

    public static class TrustedScientificDownloadProviders
    {
        public static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "core.ac.uk",
            "arxiv.org"
        };
    }

     public static class TrustedScientificDoIProviders
    {
        public static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "doi.org",
        };
    }
}