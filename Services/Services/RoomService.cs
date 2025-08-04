using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RoomService> _logger;

        public RoomService(IUnitOfWork unitOfWork, ILogger<RoomService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        private RoomDto MapToRoomDto(room model)
        {
            return new RoomDto
            {
                RoomId = model.room_id,
                RoomCode = model.room_code,
                Capacity = model.capacity,
                Description = model.description
            };
        }

        public async Task<IEnumerable<RoomDto>> GetAllAsync()
        {
            var rooms = await _unitOfWork.Rooms.GetAllAsync();
            return rooms.Select(MapToRoomDto);
        }

        public async Task<RoomDto> GetByIdAsync(int id)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            if (room == null)
            {
                throw new NotFoundException("Room", "Id", id);
            }
            return MapToRoomDto(room);
        }

        public async Task<RoomDto> AddAsync(CreateRoomDto createRoomDto)
        {
            var existingRoom = await _unitOfWork.Rooms.FindOneAsync(r => r.room_code == createRoomDto.RoomCode);
            if (existingRoom != null)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "RoomCode", new[] { $"Mã phòng '{createRoomDto.RoomCode}' đã tồn tại." } }
                });
            }

            var roomEntity = new room
            {
                room_code = createRoomDto.RoomCode,
                capacity = createRoomDto.Capacity,
                description = createRoomDto.Description
            };

            try
            {
                var addedRoom = await _unitOfWork.Rooms.AddAsync(roomEntity);
                await _unitOfWork.CompleteAsync();
                return MapToRoomDto(addedRoom);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException during Room AddAsync.");
                if (dbEx.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true ||
                    (dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505"))
                {
                    throw new ApiException($"Mã phòng '{createRoomDto.RoomCode}' đã tồn tại.", (int)HttpStatusCode.Conflict);
                }
                throw new ApiException("Có lỗi xảy ra khi thêm phòng vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during Room AddAsync.");
                throw new ApiException("Đã xảy ra lỗi không mong muốn khi thêm phòng.", ex, (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task UpdateAsync(UpdateRoomDto updateRoomDto)
        {
            var existingRoom = await _unitOfWork.Rooms.GetByIdAsync(updateRoomDto.RoomId);
            if (existingRoom == null)
            {
                throw new NotFoundException("Room", "Id", updateRoomDto.RoomId);
            }

            if (!string.IsNullOrEmpty(updateRoomDto.RoomCode) && updateRoomDto.RoomCode != existingRoom.room_code)
            {
                var roomWithSameCode = await _unitOfWork.Rooms.FindOneAsync(r => r.room_code == updateRoomDto.RoomCode);
                if (roomWithSameCode != null && roomWithSameCode.room_id != updateRoomDto.RoomId)
                    {
                        throw new ValidationException(new Dictionary<string, string[]>
                        {
                            { "RoomCode", new[] { $"Mã phòng '{updateRoomDto.RoomCode}' đã tồn tại và được sử dụng bởi phòng khác." } }
                        });
                    }
                }

                if (updateRoomDto.RoomCode != null)
                {
                    existingRoom.room_code = updateRoomDto.RoomCode;
                }
                if (updateRoomDto.Capacity.HasValue)
                {
                    existingRoom.capacity = updateRoomDto.Capacity.Value;
                }
                if (updateRoomDto.Description != null)
                {
                    existingRoom.description = updateRoomDto.Description;
                }

                try
                {
                    await _unitOfWork.Rooms.UpdateAsync(existingRoom);
                    await _unitOfWork.CompleteAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "DbUpdateException during Room UpdateAsync.");
                     if (dbEx.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true ||
                        (dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505"))
                    {
                        throw new ApiException($"Mã phòng '{existingRoom.room_code}' đã tồn tại.", (int)HttpStatusCode.Conflict);
                    }
                    throw new ApiException("Có lỗi xảy ra khi cập nhật phòng trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred during Room UpdateAsync.");
                    throw new ApiException("Đã xảy ra lỗi không mong muốn khi cập nhật phòng.", ex, (int)HttpStatusCode.InternalServerError);
                }
            }

            public async Task DeleteAsync(int id)
            {
                var roomToDelete = await _unitOfWork.Rooms.GetByIdAsync(id);
                if (roomToDelete == null)
                {
                    throw new NotFoundException("Room", "Id", id);
                }

                try
                {
                    await _unitOfWork.Rooms.DeleteAsync(id);
                    await _unitOfWork.CompleteAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, $"DbUpdateException during Room DeleteAsync for ID {id}.");
                    throw new ApiException("Không thể xóa phòng do có các bản ghi liên quan (ràng buộc khóa ngoại).", dbEx, (int)HttpStatusCode.Conflict);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred during Room DeleteAsync.");
                    throw new ApiException("Đã xảy ra lỗi không mong muốn khi xóa phòng.", ex, (int)HttpStatusCode.InternalServerError);
                }
            }

            public async Task<IEnumerable<RoomDto>> SearchRoomsAsync(string? roomCode = null, string? description = null)
            {
                var allRooms = await _unitOfWork.Rooms.GetAllAsync();

                var filteredRooms = allRooms.Where(r =>
                    (string.IsNullOrEmpty(roomCode) || (r.room_code != null && r.room_code.Contains(roomCode, StringComparison.OrdinalIgnoreCase))) &&
                    (string.IsNullOrEmpty(description) || (r.description != null && r.description.Contains(description, StringComparison.OrdinalIgnoreCase)))
                );

                return filteredRooms.Select(MapToRoomDto);
            }
        }