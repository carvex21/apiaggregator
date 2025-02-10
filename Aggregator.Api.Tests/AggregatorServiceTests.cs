using Aggregator.Api.Models;
using Aggregator.Api.Services;
using Moq;
using Shouldly;

namespace Aggregator.Api.Tests
{
    [TestFixture]
    public class AggregatorServiceTests
    {
        private Mock<IGeoapifyService> _geoapifyServiceMock;
        private Mock<IWeatherService> _weatherServiceMock;
        private Mock<INewsService> _newsServiceMock;
        private Mock<IPlacesService> _placesServiceMock;
        private AggregatorService _aggregatorService;

        [SetUp]
        public void SetUp()
        {
            _geoapifyServiceMock = new Mock<IGeoapifyService>();
            _weatherServiceMock = new Mock<IWeatherService>();
            _newsServiceMock = new Mock<INewsService>();
            _placesServiceMock = new Mock<IPlacesService>();

            _aggregatorService = new AggregatorService(
                _geoapifyServiceMock.Object,
                _weatherServiceMock.Object,
                _newsServiceMock.Object,
                _placesServiceMock.Object);
        }

        [Test]
        public async Task GetAggregatedDataAsync_ReturnsNull_WhenLocationNotFound()
        {
            var city = "Unknown";

            _geoapifyServiceMock
                .Setup(x => x.GetLocationDataAsync(city))
                .ReturnsAsync((LocationData)null);

            var result = await _aggregatorService.GetAggregatedDataAsync(city);

            result.ShouldBeNull();
        }

        [Test]
        public async Task GetAggregatedDataAsync_HandlesMissingWeatherData()
        {
            var city = "Athens";

            _geoapifyServiceMock
                .Setup(x => x.GetLocationDataAsync(city))
                .ReturnsAsync(new LocationData("Athens", 37.9838, 23.7275));

            _weatherServiceMock
                .Setup(x => x.GetWeatherAsync(37.9838, 23.7275))
                .ReturnsAsync((WeatherData)null);

            _newsServiceMock
                .Setup(x => x.GetNewsAsync(city))
                .ReturnsAsync(new List<NewsArticle>());

            _placesServiceMock
                .Setup(x => x.GetPlacesAsync(city))
                .ReturnsAsync(new List<PlaceData>());

            var result = await _aggregatorService.GetAggregatedDataAsync(city);

            result.ShouldNotBeNull();
        }

        [Test]
        public async Task GetAggregatedDataAsync_HandlesMissingNewsData()
        {
            var city = "Athens";

            _geoapifyServiceMock
                .Setup(x => x.GetLocationDataAsync(city))
                .ReturnsAsync(new LocationData("Athens", 37.9838, 23.7275));

            _weatherServiceMock
                .Setup(x => x.GetWeatherAsync(37.9838, 23.7275))
                .ReturnsAsync(new WeatherData { Temperature = 22 });

            _newsServiceMock
                .Setup(x => x.GetNewsAsync(city))
                .ReturnsAsync((List<NewsArticle>)null);

            _placesServiceMock
                .Setup(x => x.GetPlacesAsync(city))
                .ReturnsAsync(new List<PlaceData>());

            var result = await _aggregatorService.GetAggregatedDataAsync(city);

            result.ShouldNotBeNull();
        }
    }
}
