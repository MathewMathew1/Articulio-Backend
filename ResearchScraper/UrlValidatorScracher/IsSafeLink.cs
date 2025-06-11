public static class UrlValidator
{
    public static bool IsTrustedProviderUrl(string url, HashSet<string> trustedProviders)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        var host = uri.Host.ToLowerInvariant();

        if (host.StartsWith("www."))
            host = host[4..];

        return trustedProviders.Contains(host);
    }
}
