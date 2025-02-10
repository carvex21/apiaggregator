using System.Net;
using System.Text;
using Aggregator.Api.Models;
using Aggregator.Api.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Shouldly;

namespace Aggregator.Api.Tests
{
    [TestFixture]
    public class NewsServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private NewsService _newsService;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://newsapi.org/v2/everything")
            };

            var apiSettingsMock = new Mock<IOptions<ApiSettings>>();
            apiSettingsMock.Setup(x => x.Value).Returns(new ApiSettings
            {
                NewsApiKey = "test-api-key"
            });

            _newsService = new NewsService(_httpClient, apiSettingsMock.Object);
        }

        [Test]
        public void Constructor_ThrowsException_WhenApiKeyIsMissing()
        {
            // Arrange
            var apiSettingsMock = new Mock<IOptions<ApiSettings>>();
            apiSettingsMock.Setup(x => x.Value).Returns(new ApiSettings { NewsApiKey = null });

            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new NewsService(new HttpClient(), apiSettingsMock.Object));
        }

        [Test]
        public async Task GetNewsAsync_ReturnsListOfArticles_WhenApiCallSucceeds()
        {
            // Arrange
            var fakeResponse = new
            {
                status = "ok",
                totalResults = 1,
                articles = new[]
                {
                    new
                    {
                        author = "James Bond",
                        title = "Sample News Title",
                        description = "Sample News Description",
                        url = "https://example.com/sample-news",
                        publishedAt = "2025-02-05T14:59:35Z",
                        content = "Sample news content."
                    }
                }
            };

            var jsonResponse = JsonConvert.SerializeObject(fakeResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _newsService.GetNewsAsync("technology");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBeOfType<List<NewsArticle>>();
            result.Count.ShouldBe(1);
            result[0].Title.ShouldBe("Sample News Title");
            result[0].Author.ShouldBe("James Bond");
            result[0].Description.ShouldBe("Sample News Description");
            result[0].Url.ShouldBe("https://example.com/sample-news");
            result[0].Content.ShouldBe("Sample news content.");
        }
    }
}
