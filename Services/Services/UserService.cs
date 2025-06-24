using System.Net;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class UserService : IUserService
{
    // private readonly IUserRepository _userRepository;
    // private readonly IRoleRepository _roleRepository;
    // private readonly IStatisticRepository _statisticRepository;
    // private readonly IOpeningScheduleRepository _openingScheduleRepository;
    // private readonly IScheduleRepository _scheduleRepository;
    // private readonly IFileStorageService _fileStorageService; // Inject IFileStorageService
    //
    // public UserService(IUserRepository userRepository,
    //                    IRoleRepository roleRepository,
    //                    IStatisticRepository statisticRepository,
    //                    IOpeningScheduleRepository openingScheduleRepository,
    //                    IScheduleRepository scheduleRepository,
    //                    IFileStorageService fileStorageService) // Thêm IFileStorageService vào constructor
    // {
    //     _userRepository = userRepository;
    //     _roleRepository = roleRepository;
    //     _statisticRepository = statisticRepository;
    //     _openingScheduleRepository = openingScheduleRepository;
    //     _scheduleRepository = scheduleRepository;
    //     _fileStorageService = fileStorageService; // Khởi tạo
    // }
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    
    public UserService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    // Phương thức Login (sẽ kiểm tra mật khẩu đã hash)
    public async Task<user> GetUserAccount(string username, string password)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(username);
        if (user == null)
        {
            throw new UnauthorizedAppException("Tên đăng nhập hoặc mật khẩu không hợp lệ.");
        }

        // --- ĐÃ THAY ĐỔI: So sánh mật khẩu THÔ trực tiếp (RẤT KHÔNG AN TOÀN) ---
        if (user.password == password) // KHÔNG HASH - RẤT RỦI RO BẢO MẬT!
        {
            return user;
        }
        throw new UnauthorizedAppException("Tên đăng nhập hoặc mật khẩu không hợp lệ.");
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return users.Select(u => MapToUserDto(u));
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User", "Id", id);
        }
        return MapToUserDto(user);
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(username);
        if (user == null)
        {
            throw new NotFoundException("User", "Username", username);
        }
        return MapToUserDto(user);
    }

    

    // UPDATE User
    public async Task UpdateAsync(
        int userId,
        string? username,
        string? accountName,
        string? newPassword, // Mật khẩu mới (nếu có)
        string? address,
        string? phoneNumber,
        bool? isDisabled,
        IFormFile? avatarImageFile, // Có thể là null
        DateOnly? birthday,
        int? roleId,
        int? statisticId,
        int? openingScheduleId,
        int? scheduleId)
    {
        var existingUser = await _unitOfWork.Users.GetByIdAsync(userId);
        if (existingUser == null)
        {
            throw new NotFoundException("User", "Id", userId);
        }

        // Xử lý file ảnh mới nếu có
        if (avatarImageFile != null && avatarImageFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(existingUser.avatar_url))
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(existingUser.avatar_url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting old avatar for user {userId}: {ex.Message}");
                    // Ghi log nhưng không ném để không chặn việc cập nhật user
                }
            }
            string newAvatarUrl = await _fileStorageService.SaveFileAsync(avatarImageFile, "avatars");
            existingUser.avatar_url = newAvatarUrl;
        }
        
        // Cập nhật các trường khác
        if (!string.IsNullOrEmpty(username))
        {
            if (existingUser.username != username)
            {
                var userWithSameUsername = await _unitOfWork.Users.GetByUsernameAsync(username);
                if (userWithSameUsername != null && userWithSameUsername.user_id != userId)
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        { "Username", new string[] { $"Tên đăng nhập '{username}' đã tồn tại cho người dùng khác." } }
                    });
                }
            }
            existingUser.username = username;
        }

        if (!string.IsNullOrEmpty(accountName)) { existingUser.account_name = accountName; }
        if (!string.IsNullOrEmpty(newPassword)) // Nếu có mật khẩu mới, cập nhật nó THÔ
        {
            // --- ĐÃ THAY ĐỔI: Lưu mật khẩu THÔ mới (RẤT KHÔNG AN TOÀN) ---
            existingUser.password = newPassword; // KHÔNG HASH - RẤT RỦI RO BẢO MẬT!
        }
        if (!string.IsNullOrEmpty(address)) { existingUser.address = address; }
        if (!string.IsNullOrEmpty(phoneNumber)) { existingUser.phone_number = phoneNumber; }
        if (isDisabled.HasValue) { existingUser.is_disabled = isDisabled.Value; }
        if (birthday.HasValue) { existingUser.birthday = birthday.Value; }

        // Cập nhật khóa ngoại Role
        if (roleId.HasValue)
        {
            if (existingUser.role_id != roleId.Value)
            {
                var roleExists = await _unitOfWork.Roles.GetByIdAsync(roleId.Value);
                if (roleExists == null)
                {
                    throw new NotFoundException("Role", "Id", roleId.Value);
                }
                existingUser.role_id = roleId.Value;
            }
        }
        else if (roleId == null && existingUser.role_id != null)
        {
            existingUser.role_id = null;
        }

        // Cập nhật khóa ngoại Statistic
        if (statisticId.HasValue)
        {
            if (existingUser.statistic_id != statisticId.Value)
            {
                var statisticExists = await _unitOfWork.Statistics.GetByIdAsync(statisticId.Value);
                if (statisticExists == null)
                {
                    throw new NotFoundException("Statistic", "Id", statisticId.Value);
                }
                existingUser.statistic_id = statisticId.Value;
            }
        }
        else if (statisticId == null && existingUser.statistic_id != null)
        {
            existingUser.statistic_id = null;
        }

        // Cập nhật khóa ngoại OpeningSchedule
        if (openingScheduleId.HasValue)
        {
            if (existingUser.opening_schedule_id != openingScheduleId.Value)
            {
                var openingScheduleExists = await _unitOfWork.OpeningSchedules.GetByIdAsync(openingScheduleId.Value);
                if (openingScheduleExists == null)
                {
                    throw new NotFoundException("Opening Schedule", "Id", openingScheduleId.Value);
                }
                existingUser.opening_schedule_id = openingScheduleId.Value;
            }
        }
        else if (openingScheduleId == null && existingUser.opening_schedule_id != null)
        {
            existingUser.opening_schedule_id = null;
        }

        // Cập nhật khóa ngoại Schedule
        if (scheduleId.HasValue)
        {
            if (existingUser.schedule_id != scheduleId.Value)
            {
                var scheduleExists = await _unitOfWork.Schedules.GetByIdAsync(scheduleId.Value);
                if (scheduleExists == null)
                {
                    throw new NotFoundException("Schedule", "Id", scheduleId.Value);
                }
                existingUser.schedule_id = scheduleId.Value;
            }
        }
        else if (scheduleId == null && existingUser.schedule_id != null)
        {
            existingUser.schedule_id = null;
        }

        try
        {
            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật người dùng vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred during user update.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var userToDelete = await _unitOfWork.Users.GetByIdAsync(id);
        if (userToDelete == null)
        {
            throw new NotFoundException("User", "Id", id);
        }

        // Xóa file ảnh đại diện liên quan khỏi Azure Blob trước
        if (!string.IsNullOrEmpty(userToDelete.avatar_url))
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(userToDelete.avatar_url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting avatar blob for user {id}: {ex.Message}");
                // Ghi log nhưng không ném lỗi để không chặn việc xóa user nếu file không tồn tại/có vấn đề
            }
        }

        try
        {
            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Không thể xóa người dùng do có các bản ghi liên quan (ràng buộc khóa ngoại).", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred during user deletion.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string? username = null, string? accountName = null, string? password = null,
        string? address = null, string? phoneNumber = null, bool? isDisabled = null, DateTime? createAt = null,
        DateOnly? birthday = null, int? roleId = null)
    {
        // Quan trọng: KHÔNG NÊN tìm kiếm theo mật khẩu thô trong thực tế.
        // Nếu bạn cần tìm kiếm người dùng, hãy dùng username/account_name/email.
        // Biến `password` ở đây sẽ bị bỏ qua khi thực hiện tìm kiếm trên DB.
        var users = await _unitOfWork.Users.SearchUsersAsync(username, accountName, null, address, phoneNumber, isDisabled, createAt, birthday, roleId);
        return users.Select(u => MapToUserDto(u));
    }

    private UserDto MapToUserDto(user model)
    {
        return new UserDto
        {
            UserId = model.user_id,
            Username = model.username,
            AccountName = model.account_name,
            // KHÔNG LẤY PASSWORD
            Address = model.address,
            PhoneNumber = model.phone_number,
            IsDisabled = model.is_disabled,
            CreateAt = model.create_at,
            AvatarUrl = model.avatar_url,
            Birthday = model.birthday,
            RoleId = model.role_id,
            StatisticId = model.statistic_id,
            OpeningScheduleId = model.opening_schedule_id,
            ScheduleId = model.schedule_id
        };
    }

    public async Task<UserDto> AddAsync(
        string? username,
        string? accountName,
        string password,
        string? address,
        string? phoneNumber,
        bool? isDisabled,
        IFormFile? avatarImageFile,
        DateOnly? birthday,
        int? roleId,
        int? statisticId,
        int? openingScheduleId,
        int? scheduleId)
    {
        // 1. Validation dữ liệu đầu vào đơn giản
        if (string.IsNullOrEmpty(username))
        {
             throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Username", new string[] { "Tên đăng nhập không được để trống." } }
            });
        }
        if (string.IsNullOrEmpty(password))
        {
             throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Password", new string[] { "Mật khẩu không được để trống." } }
            });
        }

        // 2. Kiểm tra trùng lặp username
        var existingUser = await _unitOfWork.Users.GetByUsernameAsync(username);
        if (existingUser != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Username", new string[] { $"Tên đăng nhập '{username}' đã tồn tại." } }
            });
        }

        // 3. Kiểm tra khóa ngoại Role
        if (roleId.HasValue)
        {
            var roleExists = await _unitOfWork.Roles.GetByIdAsync(roleId.Value);
            if (roleExists == null)
            {
                throw new NotFoundException("Role", "Id", roleId.Value);
            }
        }

        // 4. Kiểm tra các khóa ngoại khác
        if (statisticId.HasValue)
        {
            var statisticExists = await _unitOfWork.Statistics.GetByIdAsync(statisticId.Value);
            if (statisticExists == null)
            {
                throw new NotFoundException("Statistic", "Id", statisticId.Value);
            }
        }
        if (openingScheduleId.HasValue)
        {
            var openingScheduleExists = await _unitOfWork.OpeningSchedules.GetByIdAsync(openingScheduleId.Value);
            if (openingScheduleExists == null)
            {
                throw new NotFoundException("Opening Schedule", "Id", openingScheduleId.Value);
            }
        }
        if (scheduleId.HasValue)
        {
            var scheduleExists = await _unitOfWork.Schedules.GetByIdAsync(scheduleId.Value);
            if (scheduleExists == null)
            {
                throw new NotFoundException("Schedule", "Id", scheduleId.Value);
            }
        }

        // 5. Tạo entity
        var userEntity = new user
        {
            username = username,
            account_name = accountName,
            // --- ĐÃ THAY ĐỔI: Lưu mật khẩu THÔ (RẤT KHÔNG AN TOÀN) ---
            password = password, // KHÔNG HASH - RẤT RỦI RO BẢO MẬT!
            address = address,
            phone_number = phoneNumber,
            is_disabled = isDisabled ?? false,
            create_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            avatar_url = null,
            birthday = birthday,
            role_id = roleId,
            statistic_id = statisticId,
            opening_schedule_id = openingScheduleId,
            schedule_id = scheduleId
        };

        // 6. Lưu user và xử lý avatar
        try
        {
            var addedUser = await _unitOfWork.Users.AddAsync(userEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi user trước

            if (avatarImageFile != null && avatarImageFile.Length > 0)
            {
                string avatarUrl = await _fileStorageService.SaveFileAsync(avatarImageFile, "avatars");
                addedUser.avatar_url = avatarUrl;
                await _unitOfWork.Users.UpdateAsync(addedUser); // Cập nhật lại user với URL avatar
                await _unitOfWork.CompleteAsync(); // Lưu thay đổi avatar URL
            }

            return MapToUserDto(addedUser);
        }
        catch (DbUpdateException dbEx) // Bắt lỗi từ Entity Framework
        {
            if (dbEx.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "DbError", new string[] { "Dữ liệu bạn nhập đã bị trùng, vui lòng kiểm tra lại." } }
                }, dbEx);
            }
            throw new ApiException("Có lỗi xảy ra khi lưu người dùng vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex) // Bắt các lỗi không mong muốn khác
        {
            throw new ApiException("An unexpected error occurred during user creation.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }
}