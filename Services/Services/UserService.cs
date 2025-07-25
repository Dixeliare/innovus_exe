using System.Net;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IClassService _classService;
    private readonly ILogger<UserService> _logger; // Thêm logger

    public UserService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, IClassService classService, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _classService = classService;
        _logger = logger; // Khởi tạo logger
    }

    public async Task<user> GetUserAccount(string username, string password)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(username);
        if (user == null)
        {
            throw new UnauthorizedAppException("Tên đăng nhập hoặc mật khẩu không hợp lệ.");
        }

        // KHÔNG THAY ĐỔI: So sánh mật khẩu nguyên văn như ban đầu
        if (user.password == password)
        {
            return user;
        }

        throw new UnauthorizedAppException("Tên đăng nhập hoặc mật khẩu không hợp lệ.");
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        
        // Eager load các navigation properties cần thiết cho từng user
        // do GetAllAsync trong GenericRepository không có Include.
        foreach (var user in users)
        {
            if (_unitOfWork.Context != null)
            {
                await _unitOfWork.Context.Entry(user).Reference(u => u.role).LoadAsync();
                await _unitOfWork.Context.Entry(user).Reference(u => u.gender).LoadAsync();
                await _unitOfWork.Context.Entry(user).Collection(u => u.classes).LoadAsync();
            }
        }
        
        return users.Select(u => MapToUserDto(u));
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        // GetUserByIdWithClassesAndRoleAsync đã được giả định là eager load role và classes.
        // Cần đảm bảo nó cũng eager load gender.
        var user = await _unitOfWork.Users.GetUserByIdWithClassesAndRoleAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User", "Id", id);
        }

        // Nếu GetUserByIdWithClassesAndRoleAsync không tải gender, thì tải thủ công:
        if (user.gender == null && user.gender_id > 0 && _unitOfWork.Context != null)
        {
             await _unitOfWork.Context.Entry(user).Reference(u => u.gender).LoadAsync();
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
        // Eager load role, gender, classes vì GetByUsernameAsync trong GenericRepository không có include
        if (_unitOfWork.Context != null)
        {
            await _unitOfWork.Context.Entry(user).Reference(u => u.role).LoadAsync();
            await _unitOfWork.Context.Entry(user).Reference(u => u.gender).LoadAsync();
            await _unitOfWork.Context.Entry(user).Collection(u => u.classes).LoadAsync();
        }
        return MapToUserDto(user);
    }


    // UPDATE User
    public async Task UpdateAsync(
        int userId,
        string? username,
        string? accountName,
        string? newPassword, // KHÔNG THAY ĐỔI: Vẫn nhận newPassword
        string? address,
        string? phoneNumber,
        bool? isDisabled,
        IFormFile? avatarImageFile,
        DateOnly? birthday,
        int? roleId,
        int? statisticId,
        string? email,
        int genderId,
        List<int>? classIds
    )
    {
        // GetUserByIdWithClassesAndRoleAsync có vẻ phù hợp hơn để cập nhật các mối quan hệ Many-to-Many
        // và đã được giả định là eager load role, gender và classes.
        var existingUser = await _unitOfWork.Users.GetUserByIdWithClassesAndRoleAsync(userId);
        if (existingUser == null)
        {
            throw new NotFoundException("User", "Id", userId);
        }

        // Load gender nếu chưa được tải (phòng trường hợp GetUserByIdWithClassesAndRoleAsync không bao gồm nó)
        if (existingUser.gender == null && existingUser.gender_id > 0 && _unitOfWork.Context != null)
        {
            await _unitOfWork.Context.Entry(existingUser).Reference(u => u.gender).LoadAsync();
        }
        // Load role nếu chưa được tải
        if (existingUser.role == null && existingUser.role_id > 0 && _unitOfWork.Context != null)
        {
            await _unitOfWork.Context.Entry(existingUser).Reference(u => u.role).LoadAsync();
        }
        // Load classes nếu chưa được tải (nếu GetUserByIdWithClassesAndRoleAsync chưa bao gồm)
        if (_unitOfWork.Context != null)
        {
            await _unitOfWork.Context.Entry(existingUser).Collection(u => u.classes).LoadAsync();
        }

        if (existingUser.gender_id != genderId)
        {
            var genderExists = await _unitOfWork.Genders.GetByIdAsync(genderId);
            if (genderExists == null)
            {
                throw new NotFoundException("Gender", "Id", genderId);
            }

            existingUser.gender_id = genderId;
        }

        // Cập nhật username (có kiểm tra trùng lặp)
        if (!string.IsNullOrEmpty(username) && existingUser.username != username)
        {
            var userWithSameUsername = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (userWithSameUsername != null && userWithSameUsername.user_id != userId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Username", new string[] { $"Tên đăng nhập '{username}' đã tồn tại cho người dùng khác." } }
                });
            }

            existingUser.username = username;
        }

        // Cập nhật email (có kiểm tra trùng lặp)
        if (email != null && existingUser.email != email)
        {
            var userWithSameEmail = await _unitOfWork.Users.FindByEmailAsync(email);
            if (userWithSameEmail != null && userWithSameEmail.user_id != userId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Email", new string[] { $"Email '{email}' đã được sử dụng bởi tài khoản khác." } }
                });
            }

            existingUser.email = email;
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
                    _logger.LogError(ex, $"Error deleting old avatar for user {userId}: {ex.Message}");
                }
            }

            string newAvatarUrl = await _fileStorageService.SaveFileAsync(avatarImageFile, "avatars");
            existingUser.avatar_url = newAvatarUrl;
        }

        // Cập nhật các trường khác
        existingUser.account_name = accountName ?? existingUser.account_name;
        if (!string.IsNullOrEmpty(newPassword))
        {
            // ĐÃ HOÀN TÁC: Gán mật khẩu mới nguyên văn
            existingUser.password = newPassword;
        }

        existingUser.address = address ?? existingUser.address;
        existingUser.phone_number = phoneNumber ?? existingUser.phone_number;
        existingUser.is_disabled = isDisabled ?? existingUser.is_disabled;
        existingUser.birthday = birthday ?? existingUser.birthday;

        // Cập nhật khóa ngoại Role
        if (roleId.HasValue && existingUser.role_id != roleId.Value)
        {
            var roleExists = await _unitOfWork.Roles.GetByIdAsync(roleId.Value);
            if (roleExists == null)
            {
                throw new NotFoundException("Role", "Id", roleId.Value);
            }

            existingUser.role_id = roleId.Value;
        }
        else if (roleId == null && existingUser.role_id != null)
        {
            existingUser.role_id = null;
        }

        // Cập nhật khóa ngoại Statistic
        if (statisticId.HasValue && existingUser.statistic_id != statisticId.Value)
        {
            var statisticExists = await _unitOfWork.Statistics.GetByIdAsync(statisticId.Value);
            if (statisticExists == null)
            {
                throw new NotFoundException("Statistic", "Id", statisticId.Value);
            }

            existingUser.statistic_id = statisticId.Value;
        }
        else if (statisticId == null && existingUser.statistic_id != null)
        {
            existingUser.statistic_id = null;
        }

        // Logic cập nhật danh sách lớp học (Many-to-Many)
        if (classIds != null)
        {
            // Đảm bảo existingUser.classes đã được eager load.
            // Phương thức GetUserByIdWithClassesAndRoleAsync đã bao gồm .Include(u => u.classes)
            // Nếu không, cần tải thủ công: await _unitOfWork.Context.Entry(existingUser).Collection(u => u.classes).LoadAsync();
            var currentClassIds = existingUser.classes.Select(c => c.class_id).ToHashSet();
            var incomingClassIdsHashSet = classIds.ToHashSet();

            // Lớp cần loại bỏ
            var classesToRemove = existingUser.classes
                .Where(c => !incomingClassIdsHashSet.Contains(c.class_id))
                .ToList();
            foreach (var classToRemove in classesToRemove)
            {
                existingUser.classes.Remove(classToRemove);
            }

            // Lớp cần thêm vào
            var classIdsToAdd = incomingClassIdsHashSet
                .Where(id => !currentClassIds.Contains(id))
                .ToList();

            foreach (var classIdToAdd in classIdsToAdd)
            {
                var classToAdd = await _unitOfWork.Classes.GetById(classIdToAdd); // Lấy entity Class
                if (classToAdd == null)
                {
                    throw new NotFoundException("Class", "Id", classIdToAdd);
                }

                existingUser.classes.Add(classToAdd); // Thêm entity Class vào collection
            }
        }


        try
        {
            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            if (dbEx.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "DbError", new string[] { "Dữ liệu bạn nhập đã bị trùng, vui lòng kiểm tra lại." } }
                }, dbEx);
            }

            _logger.LogError(dbEx, "DbUpdateException during User UpdateAsync.");
            throw new ApiException("Có lỗi xảy ra khi cập nhật người dùng vào cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during user update.");
            throw new ApiException("An unexpected error occurred during user update.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var userToDelete = await _unitOfWork.Users.GetByIdAsync(id);
        if (userToDelete == null)
        {
            throw new NotFoundException("User", "Id", id);
        }

        if (!string.IsNullOrEmpty(userToDelete.avatar_url))
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(userToDelete.avatar_url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting avatar blob for user {id}: {ex.Message}");
            }
        }

        try
        {
            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DbUpdateException during User DeleteAsync.");
            throw new ApiException("Không thể xóa người dùng do có các bản ghi liên quan (ràng buộc khóa ngoại).", dbEx,
                (int)HttpStatusCode.Conflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during user deletion.");
            throw new ApiException("An unexpected error occurred during user deletion.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string? username = null, string? accountName = null,
        string? password = null, // Password parameter should be removed or handled differently
        string? address = null, string? phoneNumber = null, bool? isDisabled = null, DateTime? createAt = null,
        DateOnly? birthday = null, int? roleId = null, string? email = null, int? genderId = null)
    {
        // Giả định SearchUsersAsync trong UserRepository đã xử lý việc tìm kiếm và đã eager load role, gender, classes
        var users = await _unitOfWork.Users.SearchUsersAsync(username, accountName, null, address, phoneNumber, 
            isDisabled, createAt, birthday, roleId, email, genderId);
        
        // Nếu SearchUsersAsync trong UserRepository không tải các mối quan hệ, cần tải thủ công ở đây:
        foreach (var user in users)
        {
            if (_unitOfWork.Context != null)
            {
                await _unitOfWork.Context.Entry(user).Reference(u => u.role).LoadAsync();
                await _unitOfWork.Context.Entry(user).Reference(u => u.gender).LoadAsync();
                await _unitOfWork.Context.Entry(user).Collection(u => u.classes).LoadAsync();
            }
        }

        return users.Select(u => MapToUserDto(u));
    }

    private UserDto MapToUserDto(user model)
    {
        return new UserDto
        {
            UserId = model.user_id,
            Username = model.username,
            AccountName = model.account_name,
            Address = model.address,
            PhoneNumber = model.phone_number,
            IsDisabled = model.is_disabled,
            CreateAt = model.create_at,
            AvatarUrl = model.avatar_url,
            Birthday = model.birthday,
            RoleId = model.role_id,
            StatisticId = model.statistic_id,
            Role = model.role != null
                ? new RoleDto
                {
                    RoleId = model.role.role_id,
                    RoleName = model.role.role_name
                }
                : null,
            Email = model.email,
            GenderId = model.gender_id,
            Gender = model.gender != null
                ? new GenderDto
                {
                    GenderId = model.gender.gender_id,
                    GenderName = model.gender.gender_name
                }
                : null,
            // Đảm bảo model.classes được eager load khi lấy user để ánh xạ ClassIds
            ClassIds = model.classes?.Select(c => c.class_id).ToList()
        };
    }

    // CREATE User
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
        string email,
        int genderId,
        int? classId = null
    )
    {
        // 1. Validation dữ liệu đầu vào cơ bản
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
        var existingUserByUsername = await _unitOfWork.Users.GetByUsernameAsync(username);
        if (existingUserByUsername != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Username", new string[] { $"Tên đăng nhập '{username}' đã tồn tại." } }
            });
        }

        // 3. Kiểm tra trùng lặp email
        var existingUserByEmail = await _unitOfWork.Users.FindByEmailAsync(email);
        if (existingUserByEmail != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Email", new string[] { $"Email '{email}' đã được sử dụng bởi tài khoản khác." } }
            });
        }

        // 4. Kiểm tra khóa ngoại Role
        if (roleId.HasValue)
        {
            var roleExists = await _unitOfWork.Roles.GetByIdAsync(roleId.Value);
            if (roleExists == null)
            {
                throw new NotFoundException("Role", "Id", roleId.Value);
            }
        }

        // 5. Kiểm tra khóa ngoại Gender
        var genderExists = await _unitOfWork.Genders.GetByIdAsync(genderId);
        if (genderExists == null)
        {
            throw new NotFoundException("Gender", "Id", genderId);
        }

        // 6. Kiểm tra các khóa ngoại khác
        if (statisticId.HasValue)
        {
            var statisticExists = await _unitOfWork.Statistics.GetByIdAsync(statisticId.Value);
            if (statisticExists == null)
            {
                throw new NotFoundException("Statistic", "Id", statisticId.Value);
            }
        }

        // KIỂM TRA CLASSID NẾU CÓ
        if (classId.HasValue)
        {
            var classExists = await _unitOfWork.Classes.GetById(classId.Value);
            if (classExists == null)
            {
                throw new NotFoundException("Class", "Id", classId.Value);
            }
        }

        // Mật khẩu nguyên văn được sử dụng
        string plainTextPassword = password;

        // Xử lý avatar trước khi tạo user entity để có thể gán avatar_url trong cùng giao dịch
        string? avatarUrl = null;
        if (avatarImageFile != null && avatarImageFile.Length > 0)
        {
            avatarUrl = await _fileStorageService.SaveFileAsync(avatarImageFile, "avatars");
        }


        // 7. Tạo entity người dùng
        var userEntity = new user
        {
            username = username,
            account_name = accountName,
            password = plainTextPassword,
            address = address,
            phone_number = phoneNumber,
            is_disabled = isDisabled ?? false,
            create_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            avatar_url = avatarUrl,
            birthday = birthday,
            role_id = roleId,
            statistic_id = statisticId,
            email = email,
            gender_id = genderId
        };

        // 8. Lưu user và xử lý avatar (giờ chỉ cần CompleteAsync 1 lần)
        try
        {
            var addedUser = await _unitOfWork.Users.AddAsync(userEntity);
            await _unitOfWork.CompleteAsync();

            // GÁN USER VÀO LỚP NẾU CLASSID ĐƯỢC CUNG CẤP
            if (classId.HasValue)
            {
                await _classService.AddUsersToClassAsync(classId.Value, new List<int> { addedUser.user_id });
            }

            // Tải lại user để có các navigation properties (role, gender, classes) cho DTO trả về
            // Cần sử dụng phương thức eager load để lấy Role, Gender, và Classes
            var addedUserWithDetails = await _unitOfWork.Users.GetUserByIdWithClassesAndRoleAsync(addedUser.user_id);
            
            // Nếu GetUserByIdWithClassesAndRoleAsync không tải gender, thì tải thủ công:
            if (addedUserWithDetails != null && addedUserWithDetails.gender == null && addedUserWithDetails.gender_id > 0 && _unitOfWork.Context != null)
            {
                 await _unitOfWork.Context.Entry(addedUserWithDetails).Reference(u => u.gender).LoadAsync();
            }

            return MapToUserDto(addedUserWithDetails ?? addedUser);
        }
        catch (DbUpdateException dbEx)
        {
            if (dbEx.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "DbError", new string[] { "Dữ liệu bạn nhập đã bị trùng, vui lòng kiểm tra lại." } }
                }, dbEx);
            }

            _logger.LogError(dbEx, "DbUpdateException during User AddAsync.");
            throw new ApiException("Có lỗi xảy ra khi lưu người dùng vào cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during user creation.");
            throw new ApiException("An unexpected error occurred during user creation.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<PersonalScheduleDto> GetPersonalScheduleAsync(int userId, DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        // Giả định GetUserByIdWithClassesAndRoleAsync tải user, role, gender và collection 'classes'
        // Nhưng các collection lồng nhau (class_sessions bên trong classes) thì KHÔNG.
        var user = await _unitOfWork.Users.GetUserByIdWithClassesAndRoleAsync(userId); 
        if (user == null)
        {
            throw new NotFoundException("User", "Id", userId);
        }

        // Tải gender nếu chưa được tải (phòng trường hợp GetUserByIdWithClassesAndRoleAsync không bao gồm nó)
        if (user.gender == null && user.gender_id > 0 && _unitOfWork.Context != null)
        {
             await _unitOfWork.Context.Entry(user).Reference(u => u.gender).LoadAsync();
        }

        var personalSchedule = new PersonalScheduleDto
        {
            UserId = user.user_id,
            Username = user.username,
            AccountName = user.account_name
        };

        var filteredSessions = new List<PersonalClassSessionDto>();

        // Duyệt qua các lớp học mà người dùng thuộc về
        if (user.classes != null)
        {
            foreach (var cls in user.classes)
            {
                // Cần đảm bảo rằng các class_sessions của class này, cùng với Day, Week, TimeSlot, Room và Instrument
                // được tải đầy đủ.
                // Phương thức GetClassWithSessionsAndTimeSlotsAndDayAndWeekAndInstrumentAndRoomAsync cần được thêm vào ClassRepository
                // và được gọi ở đây.
                var fullClass = await _unitOfWork.Classes.GetClassWithSessionsAndTimeSlotsAndDayAndWeekAndInstrumentAndRoomAsync(cls.class_id);

                if (fullClass?.class_sessions != null)
                {
                    foreach (var session in fullClass.class_sessions) 
                    {
                        // Eager load các thuộc tính cần thiết của session nếu chúng chưa được tải
                        if (_unitOfWork.Context != null)
                        {
                            await _unitOfWork.Context.Entry(session).Reference(s => s.day).LoadAsync();
                            await _unitOfWork.Context.Entry(session).Reference(s => s.time_slot).LoadAsync();
                            await _unitOfWork.Context.Entry(session).Reference(s => s.room).LoadAsync(); // Tải Room
                            if (session.day != null)
                            {
                                await _unitOfWork.Context.Entry(session.day).Reference(d => d.week).LoadAsync();
                            }
                        }

                        if (startDate.HasValue && session.date < startDate.Value) continue;
                        if (endDate.HasValue && session.date > endDate.Value) continue;

                        if (session.day != null && session.day.week != null && session.time_slot != null && session.room != null) // Kiểm tra session.room
                        {
                            filteredSessions.Add(new PersonalClassSessionDto
                            {
                                ClassSessionId = session.class_session_id,
                                SessionNumber = session.session_number,
                                Date = session.date,
                                RoomCode = session.room.room_code, // SỬA LỖI Ở ĐÂY: Truy cập room_code từ session.room
                                WeekId = session.day.week.week_id,
                                ClassId = session.class_id,
                                TimeSlotId = session.time_slot_id,
                                WeekNumberInMonth = session.day.week.week_number_in_month,
                                DayOfWeekName = session.day.day_of_week_name,
                                ClassCode = fullClass.class_code, 
                                InstrumentName = fullClass.instrument?.instrument_name,
                                StartTime = session.time_slot.start_time.ToTimeSpan(),
                                EndTime = session.time_slot.end_time.ToTimeSpan()
                            });
                        }
                        else
                        {
                             _logger.LogWarning($"Skipping class session {session.class_session_id} due to missing navigation data (day, week, time_slot, or room).");
                        }
                    }
                }
            }
        }

        personalSchedule.ScheduledSessions = filteredSessions.OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToList();
        return personalSchedule;
    }
}