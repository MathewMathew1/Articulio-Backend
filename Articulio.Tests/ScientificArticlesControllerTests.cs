using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using ResearchScrapper.Api.Controller;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;


public class ScientificArticlesControllerTests
{
    private readonly Mock<ILogger<ScrapeController>> _mockLogger = new();
    private readonly Mock<IAggregateScientificPapersService> _mockService = new();
    private readonly Mock<IQuerySanitizationService> _mockSanitizer = new();
    private readonly ScientificArticlesController _controller;

    public ScientificArticlesControllerTests()
    {
        _controller = new ScientificArticlesController(_mockLogger.Object, _mockService.Object, _mockSanitizer.Object);
    }

    [Fact]
    public async Task GetSelective_ReturnsOkResult()
    {
        var dto = new GetScientificArticlesDto { Query = "test", Sources = new List<SourceScientificArticleType>() };
        _mockSanitizer.Setup(s => s.Sanitize("test")).Returns("test");
        _mockService.Setup(s => s.ScrapeAsync("test", It.IsAny<IEnumerable<SourceScientificArticleType>>(), It.IsAny<CancellationToken>(), 1, SearchTag.Tag))
                    .ReturnsAsync(new Dictionary<SourceScientificArticleType, ArticleFetchResult>());

        var result = await _controller.GetSelective(dto, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }
}

