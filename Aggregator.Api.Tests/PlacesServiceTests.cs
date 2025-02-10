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
    public class PlacesServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private PlacesService _placesService;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.geoapify.com/v2/places")
            };

            var apiSettingsMock = new Mock<IOptions<ApiSettings>>();
            apiSettingsMock.Setup(x => x.Value).Returns(new ApiSettings
            {
                GeoapifyApiKey = "test-api-key"
            });

            _placesService = new PlacesService(_httpClient, apiSettingsMock.Object);
        }

        [Test]
        public void Constructor_ThrowsException_WhenApiKeyIsMissing()
        {
            // Arrange
            var apiSettingsMock = new Mock<IOptions<ApiSettings>>();
            apiSettingsMock.Setup(x => x.Value).Returns(new ApiSettings { GeoapifyApiKey = null });

            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new PlacesService(new HttpClient(), apiSettingsMock.Object));
        }

        [Test]
        public async Task GetPlacesAsync_ReturnsListOfPlaces_WhenApiCallSucceeds()
        {
            // Arrange
            var fakeResponse = new
            {
                features = new[]
                {
                    new
                    {
                        properties = new
                        {
                            name = "Acropolis Museum",
                            formatted = "Dionysiou Areopagitou 15, Athens, Greece",
                            categories = new[] { "museum" }
                        }
                    },
                    new
                    {
                        properties = new
                        {
                            name = "National Archaeological Museum",
                            formatted = "28is Oktovriou 44, Athens, Greece",
                            categories = new[] { "museum", "historical" }
                        }
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
            var result = await _placesService.GetPlacesAsync("Athens");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBeOfType<List<PlaceData>>();
            result.Count.ShouldBe(2);
            result[0].Name.ShouldBe("Acropolis Museum");
            result[0].Address.ShouldBe("Dionysiou Areopagitou 15, Athens, Greece");
            result[1].Name.ShouldBe("National Archaeological Museum");
            result[1].Address.ShouldBe("28is Oktovriou 44, Athens, Greece");
        }
    }
}
