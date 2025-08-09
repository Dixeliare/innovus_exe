using System.Net;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Exceptions;
using Services.IServices;

namespace Web_API.Controllers;

// GET
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
// [Authorize(Roles = "1,2")] // Example: Only Admin, Manager can manage rooms
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAllRooms()
    {
        try
        {
            var rooms = await _roomService.GetAllAsync();
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An error occurred while retrieving rooms.", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoomDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<RoomDto>> GetRoomById(int id)
    {
        try
        {
            var room = await _roomService.GetByIdAsync(id);
            return Ok(room);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = $"An error occurred while retrieving room with ID {id}.", details = ex.Message });
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<IEnumerable<RoomDto>>> SearchRooms(
        [FromQuery] string? roomCode = null,
        [FromQuery] string? description = null)
    {
        try
        {
            var rooms = await _roomService.SearchRoomsAsync(roomCode, description);
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An error occurred during room search.", details = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoomDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomDto createRoomDto)
    {
        try
        {
            var newRoom = await _roomService.AddAsync(createRoomDto);
            return CreatedAtAction(nameof(GetRoomById), new { id = newRoom.RoomId }, newRoom);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An unexpected error occurred during room creation.", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDto updateRoomDto)
    {
        if (id != updateRoomDto.RoomId)
        {
            return BadRequest(new { message = "ID phòng trong URL không khớp với ID trong body." });
        }

        try
        {
            await _roomService.UpdateAsync(updateRoomDto);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An unexpected error occurred during room update.", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        try
        {
            await _roomService.DeleteAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An unexpected error occurred during room deletion.", details = ex.Message });
        }
    }
}