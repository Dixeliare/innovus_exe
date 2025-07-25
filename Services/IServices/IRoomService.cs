using DTOs;

namespace Services.IServices;

public interface IRoomService
{
    Task<IEnumerable<RoomDto>> GetAllAsync();
    Task<RoomDto> GetByIdAsync(int id);
    Task<RoomDto> AddAsync(CreateRoomDto createRoomDto);
    Task UpdateAsync(UpdateRoomDto updateRoomDto);
    Task DeleteAsync(int id);
    Task<IEnumerable<RoomDto>> SearchRoomsAsync(string? roomCode = null, string? description = null);
}