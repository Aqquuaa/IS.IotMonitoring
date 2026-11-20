using IS.IotMonitoring.CoreApi.Application.DTOs;
using IS.IotMonitoring.CoreApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using Serilog.Core;

namespace IS.IotMonitoring.CoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryService telemetryService;
        private readonly ILogger<TelemetryController> logger;
        private static readonly Gauge temperature = Metrics.CreateGauge(
            "iot_device_temperature_celsius", "Latest temperature per device", "device_id");

        public TelemetryController(ITelemetryService telemetryService, ILogger<TelemetryController> logger)
        {
            this.telemetryService = telemetryService;
            this.logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> Record([FromBody] RecordTelemetryRequest request)
        {
            var result = await telemetryService.RecordTelemetry(request);
            if (!result.IsSuccess)
                return NotFound(result.Error?.Message);

            if (request.Reading > 35)
                logger.LogWarning("High temperature {Reading} °C from device {DeviceId}", request.Reading, request.DeviceId);

            temperature.WithLabels(request.DeviceId.ToString()).Set(request.Reading!.Value);
            return Ok();
        }
        [HttpGet("{deviceId:guid}")]
        public async Task<IActionResult> GetByDevice(
            Guid deviceId,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            var result = await telemetryService.GetDeviceTelemetry(deviceId, from, to);
            if (!result.IsSuccess)
                return NotFound(result.Error?.Message);

            return Ok(result.Value);
        }

    }
}
