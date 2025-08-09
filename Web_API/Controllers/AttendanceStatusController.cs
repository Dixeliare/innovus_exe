using Microsoft.AspNetCore.Mvc;
using Services.IServices;

namespace Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AttendanceStatusController : ControllerBase
    {
        private readonly IAttendanceStatusService _service;

        public AttendanceStatusController(IAttendanceStatusService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] DTOs.UpdateAttendanceStatusDto dto)
        {
            await _service.UpdateAsync(dto);
            return NoContent();
        }
    }
}