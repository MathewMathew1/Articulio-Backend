using ResearchScrapper.Api.Models;


namespace Articulio.Tests
{

    public class UrlValidatorTests
    {
        [Fact]
        public void ValidUrl_FromTrustedProvider_ReturnsTrue()
        {
            var result = UrlValidator.IsTrustedProviderUrl("https://medium.com/some-article", TrustedProviders.AllowedHosts);
            Assert.True(result);
        }

        [Fact]
        public void ValidUrl_WithWwwPrefix_IsStrippedAndTrusted()
        {
            var result = UrlValidator.IsTrustedProviderUrl("https://www.medium.com/article", TrustedProviders.AllowedHosts);
            Assert.True(result);
        }

        [Fact]
        public void ValidUrl_UntrustedHost_ReturnsFalse()
        {
            var result = UrlValidator.IsTrustedProviderUrl("https://nottrusted.com/hack", TrustedProviders.AllowedHosts);
            Assert.False(result);
        }

        [Fact]
        public void InvalidUrlFormat_ReturnsFalse()
        {
            var result = UrlValidator.IsTrustedProviderUrl("not a url", TrustedProviders.AllowedHosts);
            Assert.False(result);
        }

        [Fact]
        public void UrlWithSuspiciousSubdomain_ReturnFalse_EvenIfDomainMatches()
        {
            var result = UrlValidator.IsTrustedProviderUrl("https://sub.medium.com/path", TrustedProviders.AllowedHosts);
            Assert.False(result);
        }

        [Fact]
        public void ValidHttpUrl_ReturnsTrue()
        {
            var result = UrlValidator.IsTrustedProviderUrl("http://medium.com/article", TrustedProviders.AllowedHosts);
            Assert.True(result);
        }

        [Fact]
        public void IsTrustedProviderUrl_Should_ReturnFalse_For_EmptyString()
        {
            var result = UrlValidator.IsTrustedProviderUrl("", TrustedProviders.AllowedHosts);
            Assert.False(result);
        }

        [Fact]
        public void IsTrustedProviderUrl_Should_Handle_Uppercase_Hosts()
        {
            var result = UrlValidator.IsTrustedProviderUrl("https://WWW.MEDIUM.COM/some-article", TrustedProviders.AllowedHosts);
            Assert.True(result);
        }

        [Fact]
        public void InValidUrl_WithRedirect_ReturnsFalse()
        {
            var result = UrlValidator.IsTrustedProviderUrl("https://evil.com/redirect?target=https://medium.com", TrustedProviders.AllowedHosts);
            Assert.False(result);
        }

        [Fact]
        public void InValidUrl_TryingToPretendToHaveValidDomain_ReturnsFalse()
        {
            var result = UrlValidator.IsTrustedProviderUrl("https://medium.com.evil.com/hijack", TrustedProviders.AllowedHosts);
            Assert.False(result);
        }

        [Fact]
        public void ValidUrl_WithDoi_ReturnsTrue()
        {
            var result = UrlValidator.IsTrustedProviderUrl("https://doi.org/10.2307/4088939", TrustedScientificDoIProviders.AllowedHosts);
            Assert.True(result);
        }

        [Fact]
        public void ValidUrlWithCore_WithDownload_ReturnsTrue()
        {
            var result = UrlValidator.IsTrustedProviderUrl("https://core.ac.uk/download/84121844.pdf", TrustedScientificDownloadProviders.AllowedHosts);
            Assert.True(result);
        }

        [Fact]
        public void ValidUrlWithArxiv_WithDownload_ReturnsTrue()
        {
            var result = UrlValidator.IsTrustedProviderUrl("https://arxiv.org/pdf/2302.01910v1", TrustedScientificDownloadProviders.AllowedHosts);
            Assert.True(result);
        }

    }
}
