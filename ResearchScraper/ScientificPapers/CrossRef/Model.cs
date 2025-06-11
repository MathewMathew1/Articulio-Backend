using System.Text.Json.Serialization;

namespace ResearchScrapper.Api.Models
{
    internal sealed class CrossrefResponse
    {
        [JsonPropertyName("message")]
        public CrossrefMessage? Message { get; set; }
    }

    internal sealed class CrossrefMessage
    {
        [JsonPropertyName("items")]
        public List<CrossrefItem>? Items { get; set; }
    }

    internal sealed class CrossrefItem
    {
        public List<string>? Title { get; set; }
        public string? Abstract { get; set; }
        public List<CrossrefAuthor>? Author { get; set; }
        public string? DOI { get; set; }
        public string? URL { get; set; }

        [JsonPropertyName("container-title")]
        public List<string>? ContainerTitle { get; set; }

        public CrossrefIssued? Issued { get; set; }
    }

    internal sealed class CrossrefAuthor
    {
        public string? Given { get; set; }
        public string? Family { get; set; }
    }

    internal sealed class CrossrefIssued
    {
        [JsonPropertyName("date-parts")]
       public List<List<int?>>? DateParts { get; set; }
    }
}