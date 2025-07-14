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
        int? scheduleId,
        string email, // THÊM TRƯỜNG EMAIL
        int genderId,
        int? classId = null);

    // Thay đổi UpdateAsync để nhận IFormFile và các thuộc tính khác
    Task UpdateAsync(
        int userId,
        string? username,
        string? accountName,
        string? newPassword,
        string? address,
        string? phoneNumber,
        bool? isDisabled,
        IFormFile? avatarImageFile,
        DateOnly? birthday,
        int? roleId,
        int? statisticId,
        int? openingScheduleId,
        int? scheduleId,
        string? email,
        // genderId ở đây sẽ là int (nếu bạn đã thay đổi UpdateUserDto)
        // Hoặc vẫn là int? nếu bạn giữ UpdateUserDto như cũ và xử lý validation trong service
        int genderId, // Sử dụng int ở đây, phản ánh việc nó luôn bắt buộc.
        // Nếu bạn giữ UpdateUserDto là int?, thì vẫn là int? ở đây
        // và thêm validation ở đầu hàm này.
        List<int>? classIds
    );

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
        int? roleId = null,
        string? email = null, // THÊM TRƯỜNG EMAIL
        int? genderId = null);
}