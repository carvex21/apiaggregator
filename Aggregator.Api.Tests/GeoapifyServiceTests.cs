using System.Net;
using Aggregator.Api.Models;
using Aggregator.Api.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Shouldly;

namespace Aggregator.Api.Tests
{
    [TestFixture]
    public class GeoapifyServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private IGeoapifyService _geoapifyService;
        private Mock<ICacheService> _cacheServiceMock;
        private IOptions<ApiSettings> _settings;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.geoapify.com/v1/geocode/search")
            };

            _cacheServiceMock = new Mock<ICacheService>();

            _settings = Options.Create(new ApiSettings
            {
                GeoapifyApiKey = "test-api-key"
            });

            _geoapifyService = new GeoapifyService(_httpClient, _settings, _cacheServiceMock.Object);
        }

        [Test]
        public async Task GetLocationDataAsync_ShouldReturnLocationData_WhenApiResponseIsValid()
        {
            // Arrange
            string mockResponse = @"{
                ""results"": [{
                    ""city"": ""Athens"",
                    ""lat"": 37.9755,
                    ""lon"": 23.7348,
                    ""place_id"": ""123456""
                }]
            }";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockResponse)
                });

            // Act
            var result = await _geoapifyService.GetLocationDataAsync("Athens");

            // Assert
            result.ShouldNotBeNull();
            result.City.ShouldBe("Athens");
            result.Latitude.ShouldBe(37.9755);
            result.Longitude.ShouldBe(23.7348);
            result.PlaceId.ShouldBe("123456");
        }

        [Test]
        public async Task GetLocationDataAsync_ShouldReturnNull_WhenApiResponseIsInvalid()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            var result = await _geoapifyService.GetLocationDataAsync("InvalidCity");

            // Assert
            result.ShouldBeNull();
        }

        [Test]
        public async Task GetLocationDataAsync_ShouldReturnNull_WhenNoResultsAreFound()
        {
            // Arrange
            string mockResponse = @"{ ""results"": [] }";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockResponse)
                });

            // Act
            var result = await _geoapifyService.GetLocationDataAsync("Nowhere");

            // Assert
            result.ShouldBeNull();
        }

        [Test]
        public async Task GetLocationDataAsync_ShouldReturnCachedLocation_IfExistsInCache()
        {
            // Arrange
            var cachedLocation = new LocationData("Athens", 37.37, 23.23, "007007");

            _cacheServiceMock
                .Setup(c => c.GetLocationFromCache("Athens"))
                .Returns(cachedLocation);

            // Act
            var result = await _geoapifyService.GetLocationDataAsync("Athens");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBeSameAs(cachedLocation);
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
