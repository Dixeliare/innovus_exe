using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IClassSessionService
{
    Task<IEnumerable<PersonalClassSessionDto>> GetAllAsync(); // Changed to PersonalClassSessionDto
    Task<PersonalClassSessionDto> GetByIdAsync(int id);      // Changed to PersonalClassSessionDto
    Task<IEnumerable<PersonalClassSessionDto>> GetClassSessionsByClassIdAsync(int classId); // Changed to PersonalClassSessionDto
    Task<IEnumerable<PersonalClassSessionDto>> GetClassSessionsByDayIdAsync(int dayId);     // Changed to PersonalClassSessionDto
    Task<BaseClassSessionDto> AddAsync(CreateClassSessionDto createClassSessionDto); // Add always returns BaseDto, then get by ID for full DTO
    Task UpdateAsync(UpdateClassSessionDto updateClassSessionDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<PersonalClassSessionDto>> SearchClassSessionsAsync(
        int? sessionNumber = null,
        DateOnly? date = null,
        int? roomId = null, // <--- ĐÃ SỬA Ở ĐÂY!
        int? classId = null,
        int? dayId = null,
        int? timeSlotId = null
    );
    Task<IEnumerable<UserDto>> GetUsersInClassSessionAsync(int classSessionId);
}