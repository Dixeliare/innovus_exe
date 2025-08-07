using DTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Exceptions;
using Services.IServices;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DayOfWeekLookupController : ControllerBase
{
    private readonly IDayOfWeekLookupService _dayOfWeekLookupService;
    private readonly ILogger<DayOfWeekLookupController> _logger; // Logger cho các hoạt động của controller

    public DayOfWeekLookupController(IDayOfWeekLookupService dayOfWeekLookupService, ILogger<DayOfWeekLookupController> logger)
    {
        _dayOfWeekLookupService = dayOfWeekLookupService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DayOfWeekLookupDto>>> GetAllDayOfWeekLookups()
    {
        // Không cần try-catch ở đây.
        // Bất kỳ Exception nào từ service sẽ được ExceptionHandlingMiddleware bắt và xử lý.
        var dayOfWeekLookups = await _dayOfWeekLookupService.GetAllDayOfWeekLookupsAsync();
        return Ok(dayOfWeekLookups);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DayOfWeekLookupDto>> GetDayOfWeekLookupById(int id)
    {
        // Không cần try-catch ở đây.
        // Nếu service ném NotFoundException, middleware sẽ bắt và trả về 404.
        var dayOfWeekLookup = await _dayOfWeekLookupService.GetDayOfWeekLookupByIdAsync(id);
        return Ok(dayOfWeekLookup);
    }
}