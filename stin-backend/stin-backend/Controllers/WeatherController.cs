using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using stin_backend.Models;

namespace stin_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public WeatherController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("GetWeather")]
        public async Task<IActionResult> GetWeather([FromQuery] string searchedCity)
        {
            string apiAdress = "https://api.openweathermap.org/data/2.5/weather?units=metric&q=";
            string apiKey = "99cbbc452293ccefcc5dda5b3ad9dc15";
            string requestUri = $"{apiAdress}{searchedCity}&appid={apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return Ok(jsonResponse);
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }

        [HttpGet("GetWeatherHistory")]
        public async Task<IActionResult> GetWeatherHistory(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] int startYear,
            [FromQuery] int startMonth,
            [FromQuery] int startDay,
            [FromQuery] int endYear,
            [FromQuery] int endMonth,
            [FromQuery] int endDay)
        {
            string historyApi = "https://api.open-meteo.com/v1/forecast?";
            string historyApiSet = "&daily=temperature_2m_max,temperature_2m_min,precipitation_sum,rain_sum,showers_sum,snowfall_sum&timezone=auto&";
            string startDate = $"{startYear}-{startMonth:D2}-{startDay:D2}";
            string endDate = $"{endYear}-{endMonth:D2}-{endDay:D2}";
            string requestUri = $"{historyApi}latitude={latitude}&longitude={longitude}{historyApiSet}start_date={startDate}&end_date={endDate}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return Ok(jsonResponse);
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }
    }
}
