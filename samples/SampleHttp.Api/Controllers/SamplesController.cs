using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ogu.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleHttp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SamplesController : ControllerBase
    {
        private static readonly string[] Samples = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public SamplesController(HttpClient httpClient, ILogger<SamplesController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetSamples()
        {
            return Ok(Samples);
        }

        [HttpGet("test/correlationId")]
        public IActionResult GetCorrelationIdFromLoggingContext()
        {
            var correlationId = HttpContext.RequestServices
                .GetRequiredService<ILoggingContext>()
                .Get(LoggingConstants.CorrelationId);

            return Ok(correlationId);
        }

        [HttpGet("test/caller-info-sample")]
        public IActionResult CallerInfoSample()
        {
            _logger.LogWithCallerInfo(l => l.LogWarning("Caller info will be added into Properties ( you can see on the log file )"));

            return Ok();
        }

        [HttpGet("test/caller-info-sample-with-scope")]
        public IActionResult CallerInfoSampleWithScope()
        {
            using (_logger.BeginScopeWithCallerInfo())
            {
                _logger.LogWarning("Caller info will be included into Properties ( you can see on the log file )");

                _logger.LogError("Caller info will be included into Properties ( you can see on the log file )");
            }

            return Ok();
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