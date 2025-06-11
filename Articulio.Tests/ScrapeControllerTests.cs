using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using ResearchScrapper.Api.Controller;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;

public class ScrapeControllerTests
{
    private readonly Mock<ILogger<ScrapeController>> _mockLogger;
    private readonly Mock<IAggregateService> _mockAggregateService;
    private readonly ScrapeController _controller;

    public ScrapeControllerTests()
    {
        _mockLogger = new Mock<ILogger<ScrapeController>>();
        _mockAggregateService = new Mock<IAggregateService>();
        _controller = new ScrapeController(_mockLogger.Object, _mockAggregateService.Object);
    }

    [Fact]
    public async Task GetSelective_ReturnsOkResult_WhenServiceReturnsData()
    {
        var dto = new GetArticlesDto
        {
            Query = "test",
            Sources = new List<SourceType> { SourceType.Medium }
        };

        var expectedResult = new Dictionary<SourceType, IEnumerable<ArticleMetadata>>
        {
            { SourceType.Medium, new List<ArticleMetadata> { new ArticleMetadata { Title = "Example", Url = "https://example.com", Source="core" } } }
        };

        _mockAggregateService
            .Setup(service => service.ScrapeAsync(dto.Query, dto.Sources, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetSelective(dto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task GetSelective_ReturnsInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var dto = new GetArticlesDto
        {
            Query = "test",
            Sources = new List<SourceType> { SourceType.Medium }
        };

        _mockAggregateService
            .Setup(service => service.ScrapeAsync(It.IsAny<string>(), It.IsAny<IEnumerable<SourceType>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("Service failure"));

        // Act
        var result = await _controller.GetSelective(dto, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Contains("Unexpected error", objectResult.Value.ToString());
    }
}
