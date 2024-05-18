using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using stin_backend.Controllers;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Controller
{
    public class WeatherControllerTest
    {
        [Fact]
        public async Task GetWeatherHistory_ShouldReturnWeatherHistory_WhenSuccessful()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{'weather':'sunny'}"),
               });

            var httpClient = new HttpClient(handlerMock.Object);
            var controller = new WeatherController(httpClient);

            // Act
            var result = await controller.GetWeatherHistory(0, 0, 2024, 5, 17, 2024, 5, 18);

            // Assert
            Assert.NotNull(result);
            // Doplňte vlastní aserci podle očekávaných výsledků
        }

        [Fact]
        public async Task GetWeatherHistory_ShouldReturnStatusCode_WhenUnsuccessful()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.BadRequest,
               });

            var httpClient = new HttpClient(handlerMock.Object);
            var controller = new WeatherController(httpClient);

            // Act
            var result = await controller.GetWeatherHistory(0, 0, 2024, 5, 17, 2024, 5, 18);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Bad Request", objectResult.Value);
        }


        [Fact]
        public async Task GetWeather_ShouldReturnStatusCode_WhenUnsuccessful()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.BadRequest,
               });

            var httpClient = new HttpClient(handlerMock.Object);
            var controller = new WeatherController(httpClient);

            // Act
            var result = await controller.GetWeather("City");

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Bad Request", objectResult.Value);
        }


        [Fact]
        public async Task GetWeather_ShouldReturnWeatherData_WhenSuccessful()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{'weather':'sunny'}"),
               });

            var httpClient = new HttpClient(handlerMock.Object);
            var controller = new WeatherController(httpClient);

            // Act
            var result = await controller.GetWeather("City");

            // Assert
            Assert.NotNull(result);
            // Doplňte vlastní aserci podle očekávaných výsledků
        }
    }
}
