using DTOs;
using Microsoft.AspNetCore.Http;
using Repository.Models;

namespace Services.IServices;

public interface IUserService
{
    Task<user?> GetUserAccount(string username, string password); // Giữ nguyên cho Login (sẽ hash mật khẩu)
    Task<IEnumerable<UserDto>> GetAllAsync(); // Đổi kiểu trả về để không lộ mật khẩu
    Task<UserDto?> GetByIdAsync(int id); // Đổi kiểu trả về để không lộ mật khẩu
    Task<UserDto?> GetByUsernameAsync(string username);

    // Thay đổi AddAsync để nhận IFormFile và các thuộc tính khác
    Task<UserDto> AddAsync(
        string? username,
        string? accountName,
        string password, // Mật khẩu thô để hash
        string? address,
        string? phoneNumber,
        bool? isDisabled,
        IFormFile? avatarImageFile, // Có thể là null
        DateOnly? birthday,
        int? roleId,
        int? statisticId,
        int? openingScheduleId,
        int? scheduleId);

    // Thay đổi UpdateAsync để nhận IFormFile và các thuộc tính khác
    Task UpdateAsync(
        int userId,
        string? username,
        string? accountName,
        string? newPassword, // Mật khẩu mới (nếu có) để hash
        string? address,
        string? phoneNumber,
        bool? isDisabled,
        IFormFile? avatarImageFile, // Có thể là null
        DateOnly? birthday,
        int? roleId,
        int? statisticId,
        int? openingScheduleId,
        int? scheduleId);

    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<UserDto>> SearchUsersAsync( // Đổi kiểu trả về để không lộ mật khẩu
        string? username = null,
        string? accountName = null,
        string? password = null, // Vẫn giữ ở đây cho tham số tìm kiếm, nhưng sẽ không tìm theo hash
        string? address = null,
        string? phoneNumber = null,
        bool? isDisabled = null,
        DateTime? createAt = null,
        DateOnly? birthday = null,
        int? roleId = null);
}