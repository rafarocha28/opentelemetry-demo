using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OTM.Options;

namespace OTM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertTypeController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly TcwOptions _options;

        public AlertTypeController(HttpClient httpClient, IOptionsMonitor<TcwOptions> options)
        {
            _options = options.CurrentValue;
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(180);
            _httpClient.BaseAddress = new Uri(_options.URL);
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, "Bearer " + _options.Token);
        }

        [HttpGet]        
        public async Task<IActionResult> Get(CancellationToken token)
        {
            var response = await _httpClient
                .GetAsync("/v1/alert-type?page=1&pageSize=10", token)
                .ConfigureAwait(false);
            if (response == null)
            {
                return NotFound();
            }
            if (response.IsSuccessStatusCode)
            {                
                return Ok();
            }

            return BadRequest();
        }
    }
}