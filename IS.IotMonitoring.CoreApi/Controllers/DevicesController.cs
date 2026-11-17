using IS.IotMonitoring.CoreApi.Application.DTOs;
using IS.IotMonitoring.CoreApi.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IS.IotMonitoring.CoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService deviceService;

        public DevicesController(IDeviceService deviceService)
        {
            this.deviceService = deviceService;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeviceRequest request)
        {
            var result = await deviceService.CreateDeviceAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result.Error?.Message);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var devices = await deviceService.GetAllAsync();
            return Ok(devices);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await deviceService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(result.Error?.Message);
            return Ok(result.Value);
        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await deviceService.DeleteDeviceAsync(id);
            if (!result.IsSuccess)
                return NotFound(result.Error?.Message);
            return NoContent();
        }
    }
}
