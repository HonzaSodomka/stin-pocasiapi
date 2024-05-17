using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace stin_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(HttpClient httpClient, ILogger<WeatherController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentWeather(string city, string apiKey)
        {
            try
            {
                var apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error response from weather API: {StatusCode}, {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }

                var data = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Weather API response: {Data}", data);

                var jsonData = JObject.Parse(data);

                var result = new
                {
                    Temperature = jsonData["main"]["temp"].ToString() + " °C",
                    Humidity = jsonData["main"]["humidity"].ToString() + " %",
                    Wind = jsonData["wind"]["speed"].ToString() + " km/h",
                    City = jsonData["name"].ToString(),
                    WeatherStatus = jsonData["weather"][0]["icon"].ToString(),
                    Longitude = jsonData["coord"]["lon"].ToString(),
                    Latitude = jsonData["coord"]["lat"].ToString()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the current weather.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistoricalWeather(double latitude, double longitude, string startDate, string endDate, string apiKey)
        {
            try
            {
                var apiUrl = $"https://api.weather.com/v1/location/{latitude},{longitude}/observations/historical.json?apiKey={apiKey}&startDate={startDate}&endDate={endDate}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error response from historical weather API: {StatusCode}, {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }

                var data = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Historical weather API response: {Data}", data);

                var jsonData = JObject.Parse(data);

                var result = new
                {
                    MaxTemp = jsonData["daily"]["temperature_2m_max"],
                    MinTemp = jsonData["daily"]["temperature_2m_min"],
                    Precipitation = jsonData["daily"]["precipitation_sum"],
                    Rain = jsonData["daily"]["rain_sum"],
                    Shower = jsonData["daily"]["showers_sum"],
                    Snow = jsonData["daily"]["snowfall_sum"]
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the historical weather.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
