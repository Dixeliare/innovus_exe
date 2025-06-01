using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class UserService : IUserService
{
    private readonly UserRepository _userRepository;
    private readonly RoleRepository _roleRepository; // Để kiểm tra khóa ngoại
    private readonly StatisticRepository _statisticRepository; // Để kiểm tra khóa ngoại
    private readonly OpeningScheduleRepository _openingScheduleRepository; // Để kiểm tra khóa ngoại
    private readonly ScheduleRepository _scheduleRepository; // Để kiểm tra khóa ngoại

    public UserService(UserRepository userRepository,
        RoleRepository roleRepository,
        StatisticRepository statisticRepository,
        OpeningScheduleRepository openingScheduleRepository,
        ScheduleRepository scheduleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _statisticRepository = statisticRepository;
        _openingScheduleRepository = openingScheduleRepository;
        _scheduleRepository = scheduleRepository;
    }
    
    public async Task<IEnumerable<user>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<user> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }
    
    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<UserDto> AddAsync(CreateUserDto createUserDto)
        {
            // Kiểm tra trùng lặp username
            if (!string.IsNullOrEmpty(createUserDto.Username))
            {
                var existingUser = await _userRepository.GetByUsernameAsync(createUserDto.Username);
                if (existingUser != null)
                {
                    throw new ArgumentException($"Username '{createUserDto.Username}' already exists.");
                }
            }

            // Kiểm tra khóa ngoại Role
            if (createUserDto.RoleId.HasValue)
            {
                var roleExists = await _roleRepository.GetByIdAsync(createUserDto.RoleId.Value);
                if (roleExists == null)
                {
                    throw new KeyNotFoundException($"Role with ID {createUserDto.RoleId} not found.");
                }
            }

            // Kiểm tra khóa ngoại Statistic
            if (createUserDto.StatisticId.HasValue)
            {
                var statisticExists = await _statisticRepository.GetByIdAsync(createUserDto.StatisticId.Value);
                if (statisticExists == null)
                {
                    throw new KeyNotFoundException($"Statistic with ID {createUserDto.StatisticId} not found.");
                }
            }

            // Kiểm tra khóa ngoại OpeningSchedule
            if (createUserDto.OpeningScheduleId.HasValue)
            {
                var openingScheduleExists = await _openingScheduleRepository.GetByIdAsync(createUserDto.OpeningScheduleId.Value);
                if (openingScheduleExists == null)
                {
                    throw new KeyNotFoundException($"Opening Schedule with ID {createUserDto.OpeningScheduleId} not found.");
                }
            }

            // Kiểm tra khóa ngoại Schedule
            if (createUserDto.ScheduleId.HasValue)
            {
                var scheduleExists = await _scheduleRepository.GetByIdAsync(createUserDto.ScheduleId.Value);
                if (scheduleExists == null)
                {
                    throw new KeyNotFoundException($"Schedule with ID {createUserDto.ScheduleId} not found.");
                }
            }

            var userEntity = new user
            {
                username = createUserDto.Username,
                account_name = createUserDto.AccountName,
                password = createUserDto.Password, // TODO: Cần hash mật khẩu ở đây trong ứng dụng thực tế
                address = createUserDto.Address,
                phone_number = createUserDto.PhoneNumber,
                is_disabled = createUserDto.IsDisabled ?? false, // Mặc định là false
                create_at = DateTime.UtcNow, // Gán thời gian tạo
                avatar_url = createUserDto.AvatarUrl,
                birthday = createUserDto.Birthday,
                role_id = createUserDto.RoleId,
                statistic_id = createUserDto.StatisticId,
                opening_schedule_id = createUserDto.OpeningScheduleId,
                schedule_id = createUserDto.ScheduleId
            };

            var addedUser = await _userRepository.AddAsync(userEntity);
            return MapToUserDto(addedUser);
        }

        // UPDATE User
        public async Task UpdateAsync(UpdateUserDto updateUserDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(updateUserDto.UserId);

            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with ID {updateUserDto.UserId} not found.");
            }

            // Cập nhật các trường nếu có giá trị được cung cấp
            if (!string.IsNullOrEmpty(updateUserDto.Username))
            {
                // Kiểm tra trùng lặp username mới (nếu có thay đổi)
                if (existingUser.username != updateUserDto.Username)
                {
                    var userWithSameUsername = await _userRepository.GetByUsernameAsync(updateUserDto.Username);
                    if (userWithSameUsername != null && userWithSameUsername.user_id != updateUserDto.UserId)
                    {
                        throw new ArgumentException($"Username '{updateUserDto.Username}' already exists for another user.");
                    }
                }
                existingUser.username = updateUserDto.Username;
            }

            if (!string.IsNullOrEmpty(updateUserDto.AccountName))
            {
                existingUser.account_name = updateUserDto.AccountName;
            }
            if (!string.IsNullOrEmpty(updateUserDto.NewPassword)) // Nếu có mật khẩu mới, cập nhật nó
            {
                existingUser.password = updateUserDto.NewPassword; // TODO: Cần hash mật khẩu mới ở đây
            }
            if (!string.IsNullOrEmpty(updateUserDto.Address))
            {
                existingUser.address = updateUserDto.Address;
            }
            if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber))
            {
                existingUser.phone_number = updateUserDto.PhoneNumber;
            }
            if (updateUserDto.IsDisabled.HasValue)
            {
                existingUser.is_disabled = updateUserDto.IsDisabled.Value;
            }
            if (!string.IsNullOrEmpty(updateUserDto.AvatarUrl))
            {
                existingUser.avatar_url = updateUserDto.AvatarUrl;
            }
            if (updateUserDto.Birthday.HasValue)
            {
                existingUser.birthday = updateUserDto.Birthday.Value;
            }

            // Cập nhật khóa ngoại Role
            if (updateUserDto.RoleId.HasValue)
            {
                if (existingUser.role_id != updateUserDto.RoleId.Value) // Chỉ cập nhật nếu thay đổi
                {
                    var roleExists = await _roleRepository.GetByIdAsync(updateUserDto.RoleId.Value);
                    if (roleExists == null)
                    {
                        throw new KeyNotFoundException($"Role with ID {updateUserDto.RoleId} not found for update.");
                    }
                    existingUser.role_id = updateUserDto.RoleId.Value;
                }
            }
            else if (updateUserDto.RoleId == null) // Nếu gán null, xóa liên kết
            {
                existingUser.role_id = null;
            }

            // Cập nhật khóa ngoại Statistic
            if (updateUserDto.StatisticId.HasValue)
            {
                if (existingUser.statistic_id != updateUserDto.StatisticId.Value)
                {
                    var statisticExists = await _statisticRepository.GetByIdAsync(updateUserDto.StatisticId.Value);
                    if (statisticExists == null)
                    {
                        throw new KeyNotFoundException($"Statistic with ID {updateUserDto.StatisticId} not found for update.");
                    }
                    existingUser.statistic_id = updateUserDto.StatisticId.Value;
                }
            }
            else if (updateUserDto.StatisticId == null)
            {
                existingUser.statistic_id = null;
            }

            // Cập nhật khóa ngoại OpeningSchedule
            if (updateUserDto.OpeningScheduleId.HasValue)
            {
                if (existingUser.opening_schedule_id != updateUserDto.OpeningScheduleId.Value)
                {
                    var openingScheduleExists = await _openingScheduleRepository.GetByIdAsync(updateUserDto.OpeningScheduleId.Value);
                    if (openingScheduleExists == null)
                    {
                        throw new KeyNotFoundException($"Opening Schedule with ID {updateUserDto.OpeningScheduleId} not found for update.");
                    }
                    existingUser.opening_schedule_id = updateUserDto.OpeningScheduleId.Value;
                }
            }
            else if (updateUserDto.OpeningScheduleId == null)
            {
                existingUser.opening_schedule_id = null;
            }

            // Cập nhật khóa ngoại Schedule
            if (updateUserDto.ScheduleId.HasValue)
            {
                if (existingUser.schedule_id != updateUserDto.ScheduleId.Value)
                {
                    var scheduleExists = await _scheduleRepository.GetByIdAsync(updateUserDto.ScheduleId.Value);
                    if (scheduleExists == null)
                    {
                        throw new KeyNotFoundException($"Schedule with ID {updateUserDto.ScheduleId} not found for update.");
                    }
                    existingUser.schedule_id = updateUserDto.ScheduleId.Value;
                }
            }
            else if (updateUserDto.ScheduleId == null)
            {
                existingUser.schedule_id = null;
            }

            await _userRepository.UpdateAsync(existingUser);
        }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<user>> SearchUsersAsync(string? username = null, string? accountName = null, string? password = null,
        string? address = null, string? phoneNumber = null, bool? isDisabled = null, DateTime? createAt = null,
        DateOnly? birthday = null, int? roleId = null)
    {
        return await _userRepository.SearchUsersAsync(username, accountName, password, address, phoneNumber, isDisabled, createAt, birthday, roleId);
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
}