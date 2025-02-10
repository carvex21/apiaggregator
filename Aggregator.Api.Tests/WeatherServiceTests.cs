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
    public class WeatherServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private IWeatherService _weatherService;
        private IOptions<ApiSettings> _settings;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/weather")
            };

            _settings = Options.Create(new ApiSettings
            {
                WeatherApiKey = "test-api-key"
            });

            _weatherService = new WeatherService(_httpClient, _settings);
        }

        [Test]
        public async Task GetWeatherAsync_ReturnsNull_WhenApiCallFails()
        {
            // Arrange
            double latitude = 40.4040;
            double longitude = -74.7474;

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
            var result = await _weatherService.GetWeatherAsync(latitude, longitude);

            // Assert
            result.ShouldBeNull();
        }

        [Test]
        public void Ctor_ThrowsException_WhenApiKeyIsMissing()
        {
            // Arrange
            var invalidSettings = Options.Create(new ApiSettings
            {
                WeatherApiKey = null
            });

            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new WeatherService(_httpClient, invalidSettings));
        }
    }
}
