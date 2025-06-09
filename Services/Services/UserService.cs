using DTOs;
using Microsoft.AspNetCore.Http;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class UserService : IUserService
{
    private readonly UserRepository _userRepository;
    private readonly RoleRepository _roleRepository;
    private readonly StatisticRepository _statisticRepository;
    private readonly OpeningScheduleRepository _openingScheduleRepository;
    private readonly ScheduleRepository _scheduleRepository;
    private readonly IFileStorageService _fileStorageService; // Inject IFileStorageService

    public UserService(UserRepository userRepository,
                       RoleRepository roleRepository,
                       StatisticRepository statisticRepository,
                       OpeningScheduleRepository openingScheduleRepository,
                       ScheduleRepository scheduleRepository,
                       IFileStorageService fileStorageService) // Thêm IFileStorageService vào constructor
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _statisticRepository = statisticRepository;
        _openingScheduleRepository = openingScheduleRepository;
        _scheduleRepository = scheduleRepository;
        _fileStorageService = fileStorageService; // Khởi tạo
    }

    // Phương thức Login (sẽ kiểm tra mật khẩu đã hash)
    public async Task<user?> GetUserAccount(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            return null;
        }

        // Verify the hashed password
        // `BCrypt.Net.BCrypt.Verify` an toàn để so sánh mật khẩu thô với mật khẩu đã hash
        if (BCrypt.Net.BCrypt.Verify(password, user.password))
        {
            return user;
        }
        return null;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => MapToUserDto(u));
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null ? MapToUserDto(user) : null;
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
        var existingUser = await _userRepository.GetByIdAsync(userId);

        if (existingUser == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        // Xử lý file ảnh mới nếu có
        if (avatarImageFile != null && avatarImageFile.Length > 0)
        {
            // 1. Xóa ảnh cũ (nếu có và không rỗng)
            if (!string.IsNullOrEmpty(existingUser.avatar_url))
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(existingUser.avatar_url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting old avatar for user {userId}: {ex.Message}");
                    // Tiếp tục mà không ném lỗi để không chặn việc cập nhật
                }
            }
            // 2. Lưu ảnh mới
            string newAvatarUrl = await _fileStorageService.SaveFileAsync(avatarImageFile, "avatars");
            existingUser.avatar_url = newAvatarUrl; // Cập nhật URL mới
        }
        // Nếu avatarImageFile là null, giữ nguyên avatar_url hiện có.
        // Nếu bạn muốn client có thể "xóa" avatar, bạn cần một cơ chế riêng (ví dụ: gửi một cờ "clearAvatar").

        // Cập nhật các trường khác nếu có giá trị được cung cấp
        if (!string.IsNullOrEmpty(username))
        {
            // Kiểm tra trùng lặp username mới (nếu có thay đổi)
            if (existingUser.username != username)
            {
                var userWithSameUsername = await _userRepository.GetByUsernameAsync(username);
                if (userWithSameUsername != null && userWithSameUsername.user_id != userId)
                {
                    throw new ArgumentException($"Username '{username}' already exists for another user.");
                }
            }
            existingUser.username = username;
        }

        if (!string.IsNullOrEmpty(accountName))
        {
            existingUser.account_name = accountName;
        }
        if (!string.IsNullOrEmpty(newPassword)) // Nếu có mật khẩu mới, hash và cập nhật nó
        {
            existingUser.password = BCrypt.Net.BCrypt.HashPassword(newPassword); // HASH MẬT KHẨU MỚI
        }
        if (!string.IsNullOrEmpty(address))
        {
            existingUser.address = address;
        }
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            existingUser.phone_number = phoneNumber;
        }
        if (isDisabled.HasValue)
        {
            existingUser.is_disabled = isDisabled.Value;
        }
        // avatar_url đã được xử lý ở trên
        if (birthday.HasValue)
        {
            existingUser.birthday = birthday.Value;
        }

        // Cập nhật khóa ngoại Role
        if (roleId.HasValue)
        {
            if (existingUser.role_id != roleId.Value)
            {
                var roleExists = await _roleRepository.GetByIdAsync(roleId.Value);
                if (roleExists == null)
                {
                    throw new KeyNotFoundException($"Role with ID {roleId.Value} not found for update.");
                }
                existingUser.role_id = roleId.Value;
            }
        }
        else if (roleId == null)
        {
            existingUser.role_id = null;
        }

        // Cập nhật khóa ngoại Statistic
        if (statisticId.HasValue)
        {
            if (existingUser.statistic_id != statisticId.Value)
            {
                var statisticExists = await _statisticRepository.GetByIdAsync(statisticId.Value);
                if (statisticExists == null)
                {
                    throw new KeyNotFoundException($"Statistic with ID {statisticId.Value} not found for update.");
                }
                existingUser.statistic_id = statisticId.Value;
            }
        }
        else if (statisticId == null)
        {
            existingUser.statistic_id = null;
        }

        // Cập nhật khóa ngoại OpeningSchedule
        if (openingScheduleId.HasValue)
        {
            if (existingUser.opening_schedule_id != openingScheduleId.Value)
            {
                var openingScheduleExists = await _openingScheduleRepository.GetByIdAsync(openingScheduleId.Value);
                if (openingScheduleExists == null)
                {
                    throw new KeyNotFoundException($"Opening Schedule with ID {openingScheduleId.Value} not found for update.");
                }
                existingUser.opening_schedule_id = openingScheduleId.Value;
            }
        }
        else if (openingScheduleId == null)
        {
            existingUser.opening_schedule_id = null;
        }

        // Cập nhật khóa ngoại Schedule
        if (scheduleId.HasValue)
        {
            if (existingUser.schedule_id != scheduleId.Value)
            {
                var scheduleExists = await _scheduleRepository.GetByIdAsync(scheduleId.Value);
                if (scheduleExists == null)
                {
                    throw new KeyNotFoundException($"Schedule with ID {scheduleId.Value} not found for update.");
                }
                existingUser.schedule_id = scheduleId.Value;
            }
        }
        else if (scheduleId == null)
        {
            existingUser.schedule_id = null;
        }

        await _userRepository.UpdateAsync(existingUser);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var userToDelete = await _userRepository.GetByIdAsync(id);
        if (userToDelete == null)
        {
            return false;
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
                // Tiếp tục mà không ném lỗi để không chặn việc xóa user nếu file không tồn tại/có vấn đề
            }
        }

        return await _userRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string? username = null, string? accountName = null, string? password = null,
        string? address = null, string? phoneNumber = null, bool? isDisabled = null, DateTime? createAt = null,
        DateOnly? birthday = null, int? roleId = null)
    {
        // Quan trọng: KHÔNG NÊN tìm kiếm theo mật khẩu thô trong thực tế.
        // Nếu bạn cần tìm kiếm người dùng, hãy dùng username/account_name/email.
        // Biến `password` ở đây sẽ bị bỏ qua khi thực hiện tìm kiếm trên DB.
        var users = await _userRepository.SearchUsersAsync(username, accountName, null, address, phoneNumber, isDisabled, createAt, birthday, roleId);
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
        // 1. Kiểm tra trùng lặp username VÀ các khóa ngoại trước
        if (!string.IsNullOrEmpty(username))
        {
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
            {
                throw new ArgumentException($"Username '{username}' already exists.");
            }
        }

        // Kiểm tra khóa ngoại Role
        if (roleId.HasValue)
        {
            var roleExists = await _roleRepository.GetByIdAsync(roleId.Value);
            if (roleExists == null)
            {
                throw new KeyNotFoundException($"Role with ID {roleId.Value} not found.");
            }
        }

        // ... (Kiểm tra các khóa ngoại khác tương tự) ...
        if (statisticId.HasValue)
        {
            var statisticExists = await _statisticRepository.GetByIdAsync(statisticId.Value);
            if (statisticExists == null)
            {
                throw new KeyNotFoundException($"Statistic with ID {statisticId.Value} not found.");
            }
        }

        if (openingScheduleId.HasValue)
        {
            var openingScheduleExists = await _openingScheduleRepository.GetByIdAsync(openingScheduleId.Value);
            if (openingScheduleExists == null)
            {
                throw new KeyNotFoundException($"Opening Schedule with ID {openingScheduleId.Value} not found.");
            }
        }

        if (scheduleId.HasValue)
        {
            var scheduleExists = await _scheduleRepository.GetByIdAsync(scheduleId.Value);
            if (scheduleExists == null)
            {
                throw new KeyNotFoundException($"Schedule with ID {scheduleId.Value} not found.");
            }
        }

        // 2. Tạo entity (chưa có avatar_url)
        var userEntity = new user
        {
            username = username,
            account_name = accountName,
            password = BCrypt.Net.BCrypt.HashPassword(password),
            address = address,
            phone_number = phoneNumber,
            is_disabled = isDisabled ?? false,
            create_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), // Đã khắc phục lỗi DateTime
            avatar_url = null, // KHỞI TẠO LÀ NULL BAN ĐẦU
            birthday = birthday,
            role_id = roleId,
            statistic_id = statisticId,
            opening_schedule_id = openingScheduleId,
            schedule_id = scheduleId
        };

        // 3. THỬ lưu user vào database trước
        try
        {
            var addedUser = await _userRepository.AddAsync(userEntity);

            // 4. Nếu lưu user thành công, MỚI TIẾN HÀNH lưu ảnh
            if (avatarImageFile != null && avatarImageFile.Length > 0)
            {
                // Lưu file vào Azure Blob Storage
                string avatarUrl = await _fileStorageService.SaveFileAsync(avatarImageFile, "avatars");

                // Cập nhật URL vào userEntity đã lưu
                addedUser.avatar_url = avatarUrl;

                // Lưu lại userEntity để cập nhật avatar_url
                // Lưu ý: Cần một phương thức UpdateAsync nhẹ hơn trong UserRepository
                // hoặc đơn giản là gọi SaveChanges() trên context của repository nếu bạn quản lý context
                // Ví dụ: _context.Entry(addedUser).State = EntityState.Modified; await _context.SaveChangesAsync();
                // Hoặc tốt hơn là _userRepository.UpdateAsync(addedUser);

                // NếuUserRepository.UpdateAsync() yêu cầu cả entity, bạn cần lấy lại entity nếu AsNoTracking
                // Để đơn giản, giả định _userRepository.UpdateAsync(addedUser) hoạt động.
                await _userRepository.UpdateAsync(addedUser);
            }

            return MapToUserDto(addedUser);
        }
        catch (Exception ex)
        {
            // 5. Nếu có bất kỳ lỗi nào xảy ra trong quá trình lưu user vào DB (hoặc lưu ảnh sau đó),
            // thì userEntity không được tạo hoàn chỉnh (hoặc không được lưu).
            // VÌ ảnh chưa được lưu, NÊN không cần rollback ảnh.
            // Re-throw lỗi để Controller xử lý.
            throw;
        }
    }
}