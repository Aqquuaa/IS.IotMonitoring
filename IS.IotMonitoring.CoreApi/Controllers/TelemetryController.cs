using IS.IotMonitoring.CoreApi.Application.DTOs;
using IS.IotMonitoring.CoreApi.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IS.IotMonitoring.CoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryService _telemetryService;
        public TelemetryController(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
        }
        [HttpPost]
        public async Task<IActionResult> Record([FromBody] RecordTelemetryRequest request)
        {
            var result = await _telemetryService.RecordTelemetry(request);
            if (!result.IsSuccess)
                return NotFound(result.Error?.Message);
            return Ok();
        }
        [HttpGet("{deviceId:guid}")]
        public async Task<IActionResult> GetByDevice(
            Guid deviceId,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            var result = await _telemetryService.GetDeviceTelemetry(deviceId, from, to);
            if (!result.IsSuccess)
                return NotFound(result.Error?.Message);
            // Streams directly to the client without buffering in RAM
            return Ok(result.Value);
        }
    }
}
