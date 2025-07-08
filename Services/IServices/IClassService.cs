using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IClassService
{
    Task<IEnumerable<ClassDto>> GetAllAsync();
    Task<ClassDto> GetByIdAsync(int id);
    Task<ClassDto> AddAsync(CreateClassDto createClassDto);
    Task UpdateAsync(UpdateClassDto updateClassDto);
    Task DeleteAsync(int id);
    Task<IEnumerable<ClassDto>> SearchClassesAsync(int? instrumentId = null, string? classCode = null);
    // THÊM CÁC PHƯƠNG THỨC NÀY:
    Task<IEnumerable<UserDto>> GetAvailableStudentsAndTeachersAsync();
    Task AssignUsersToClassAsync(int classId, List<int> userIds); // Gán (thay thế)
    Task AddUsersToClassAsync(int classId, List<int> userIds); // Thêm
    Task RemoveUsersFromClassAsync(int classId, List<int> userIds); // Xóa
    
    Task<ClassDto> GetClassWithUsersByIdAsync(int id);
}