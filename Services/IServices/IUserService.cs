using DTOs;
using Microsoft.AspNetCore.Http;
using Repository.Models;

namespace Services.IServices;

public interface IUserService
{
    // Không trả về null, mà ném UnauthorizedAppException nếu không tìm thấy hoặc sai mật khẩu
    Task<user> GetUserAccount(string username, string password);

    Task<IEnumerable<UserDto>> GetAllAsync();

    // Sẽ ném NotFoundException nếu không tìm thấy, không trả về null
    Task<UserDto> GetByIdAsync(int id);

    // Sẽ ném NotFoundException nếu không tìm thấy, không trả về null
    Task<UserDto> GetByUsernameAsync(string username);

    // Thay đổi AddAsync để nhận IFormFile và các thuộc tính khác
    Task<UserDto> AddAsync(
        string? username,
        string? accountName,
        string password, // Mật khẩu thô (sẽ không hash theo yêu cầu của bạn)
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
        string? newPassword, // Mật khẩu thô mới (nếu có)
        string? address,
        string? phoneNumber,
        bool? isDisabled,
        IFormFile? avatarImageFile, // Có thể là null
        DateOnly? birthday,
        int? roleId,
        int? statisticId,
        int? openingScheduleId,
        int? scheduleId);

    // Sẽ ném NotFoundException nếu không tìm thấy, không trả về bool
    Task DeleteAsync(int id);

    Task<IEnumerable<UserDto>> SearchUsersAsync(
        string? username = null,
        string? accountName = null,
        string? password = null, // Vẫn giữ ở đây cho tham số tìm kiếm, nhưng sẽ không tìm theo mật khẩu thô trong DB
        string? address = null,
        string? phoneNumber = null,
        bool? isDisabled = null,
        DateTime? createAt = null,
        DateOnly? birthday = null,
        int? roleId = null);
}