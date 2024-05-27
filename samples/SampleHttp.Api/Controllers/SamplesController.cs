using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ogu.Extensions.Logging.Abstractions;

namespace SampleHttp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SamplesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        public SamplesController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static readonly string[] Samples = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public IActionResult GetSamples()
        {
            return Ok(HttpContext.RequestServices.GetRequiredService<ILoggingContext>().Get("CorrelationId"));
        }

        [HttpGet("test/http-client-logging")]
        public async Task<IActionResult> TestHttpClientLogging()
        {
            var url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/samples";

            var response = await _httpClient.GetStringAsync(url);

            return Ok(response);
        }
    }
}