using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class ClassService : IClassService
{
    // private readonly IClassRepository _classRepository;
    //
    // public ClassService (IClassRepository classRepository) => _classRepository = classRepository;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IStatisticService _statisticService;

    public ClassService(IUnitOfWork unitOfWork, IStatisticService statisticService)
    {
        _unitOfWork = unitOfWork;
        _statisticService = statisticService;
    }

    public async Task<IEnumerable<ClassDto>> GetAllAsync()
    {
        var classes = await _unitOfWork.Classes.GetAll();
        return classes.Select(MapToClassDto);
    }

    public async Task<ClassDto> GetByIdAsync(int id)
    {
        var cls = await _unitOfWork.Classes.GetById(id);
        if (cls == null)
        {
            throw new NotFoundException("Class", "Id", id);
        }

        return MapToClassDto(cls);
    }

    public async Task<ClassDto> AddAsync(CreateClassDto createClassDto)
    {
        var instrumentExists = await _unitOfWork.Instruments.GetByIdAsync(createClassDto.InstrumentId);
        if (instrumentExists == null)
        {
            throw new NotFoundException("Instrument", "Id", createClassDto.InstrumentId);
        }

        if (!string.IsNullOrEmpty(createClassDto.ClassCode))
        {
            var existingClass = await _unitOfWork.Classes.FindOneAsync(c =>
                c.class_code != null && c.class_code.ToLower() == createClassDto.ClassCode.ToLower());
            if (existingClass != null)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ClassCode", new[] { $"Mã lớp học '{createClassDto.ClassCode}' đã tồn tại." } }
                });
            }
        }

        var classEntity = new _class
        {
            class_code = createClassDto.ClassCode,
            instrument_id = createClassDto.InstrumentId
        };

        try
        {
            // Chỉ thêm entity vào DbContext, chưa lưu vào DB
            var addedClass = await _unitOfWork.Classes.AddAsync(classEntity);

            // LƯU Ý: Sau khi addedClass được thêm vào DbContext nhưng chưa SaveChanges(),
            // addedClass.class_id có thể chưa có giá trị từ database nếu nó là IDENTITY/SERIAL.
            // Để có class_id hợp lệ cho việc GetById sau này, bạn phải gọi CompleteAsync() trước.
            // Hoặc đơn giản là truy cập addedClass.class_id sau khi CompleteAsync() thành công.

            await _unitOfWork.CompleteAsync(); // Lưu tất cả thay đổi vào DB một lần

            // Bây giờ addedClass.class_id đã có giá trị từ DB
            // Bạn có thể fetch lại hoặc nếu cần tên instrument,
            // có thể lấy từ instrumentExists nếu nó không thay đổi.
            // Hoặc fetch lại nếu entity có các navigation properties cần được load.
            var addedClassWithInstrument = await _unitOfWork.Classes.GetById(addedClass.class_id);
            if (addedClassWithInstrument == null)
            {
                throw new ApiException("Failed to retrieve newly added class with instrument data.", null,
                    (int)HttpStatusCode.InternalServerError);
            }

            // Cập nhật thống kê sau khi thêm lớp học
            await _statisticService.UpdateStatisticsOnClassChangeAsync();
            
            return MapToClassDto(addedClassWithInstrument);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm lớp học vào cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the class.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateClassDto updateClassDto)
    {
        // Dùng GetById để đảm bảo tải cả Instrument nếu cần dùng trong logic sau này
        var existingClass = await _unitOfWork.Classes.GetById(updateClassDto.ClassId);

        if (existingClass == null)
        {
            throw new NotFoundException("Class", "Id", updateClassDto.ClassId);
        }

        // Kiểm tra và cập nhật InstrumentId
        if (updateClassDto.InstrumentId.HasValue && updateClassDto.InstrumentId.Value != existingClass.instrument_id)
        {
            var instrumentExists = await _unitOfWork.Instruments.GetByIdAsync(updateClassDto.InstrumentId.Value);
            if (instrumentExists == null)
            {
                throw new NotFoundException("Instrument", "Id", updateClassDto.InstrumentId.Value);
            }

            existingClass.instrument_id = updateClassDto.InstrumentId.Value;
        }

        // Kiểm tra tính duy nhất của ClassCode nếu ClassCode được cập nhật
        if (!string.IsNullOrEmpty(updateClassDto.ClassCode) &&
            updateClassDto.ClassCode.ToLower() != existingClass.class_code?.ToLower())
        {
            var classWithSameCode = await _unitOfWork.Classes.FindOneAsync(c =>
                c.class_code != null && c.class_code.ToLower() == updateClassDto.ClassCode.ToLower());
            if (classWithSameCode != null && classWithSameCode.class_id != updateClassDto.ClassId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    {
                        "ClassCode",
                        new[] { $"Mã lớp học '{updateClassDto.ClassCode}' đã được sử dụng bởi một lớp học khác." }
                    }
                });
            }

            existingClass.class_code = updateClassDto.ClassCode;
        }

        try
        {
            await _unitOfWork.Classes.UpdateAsync(existingClass);
            await _unitOfWork.CompleteAsync();
            
            // Cập nhật thống kê sau khi cập nhật lớp học
            await _statisticService.UpdateStatisticsOnClassChangeAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật lớp học trong cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the class.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var classToDelete = await _unitOfWork.Classes.GetByIdAsync(id);
        if (classToDelete == null)
        {
            throw new NotFoundException("Class", "Id", id);
        }

        try
        {
            var hasRelatedSessions = await _unitOfWork.ClassSessions.AnyAsync(cs => cs.class_id == id);
            if (hasRelatedSessions)
            {
                throw new ApiException("Không thể xóa lớp học này vì có các buổi học liên quan.", null,
                    (int)HttpStatusCode.Conflict);
            }

            var hasRelatedUsers = await _unitOfWork.Users.AnyAsync(u => u.classes.Any(c => c.class_id == id));
            if (hasRelatedUsers)
            {
                throw new ApiException(
                    "Không thể xóa lớp học này vì có người dùng (học viên/giáo viên) đang tham gia lớp.", null,
                    (int)HttpStatusCode.Conflict);
            }

            await _unitOfWork.Classes.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            
            // Cập nhật thống kê sau khi xóa lớp học
            await _statisticService.UpdateStatisticsOnClassChangeAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi xóa lớp học khỏi cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the class.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<ClassDto>> SearchClassesAsync(int? instrumentId = null, string? classCode = null)
    {
        var classes = await _unitOfWork.Classes.SearchClassesAsync(instrumentId, classCode);
        return classes.Select(MapToClassDto);
    }

    private ClassDto MapToClassDto(_class model)
    {
        return new ClassDto
        {
            ClassId = model.class_id,
            ClassCode = model.class_code,
            InstrumentId = model.instrument_id,
            Instrument = model.instrument != null ? new InstrumentDto
            {
                InstrumentId = model.instrument.instrument_id,
                InstrumentName = model.instrument.instrument_name
            } : null,
            // THÊM LOGIC NÀY ĐỂ ÁNH XẠ DANH SÁCH NGƯỜI DÙNG
            Users = model.users?.Select(u => new UserDto
            {
                UserId = u.user_id,
                Username = u.username,
                AccountName = u.account_name,
                // KHÔNG BAO GỒM PASSWORD Ở ĐÂY
                Address = u.address,
                PhoneNumber = u.phone_number,
                IsDisabled = u.is_disabled,
                CreateAt = u.create_at,
                AvatarUrl = u.avatar_url,
                Birthday = u.birthday,
                RoleId = u.role_id,
                StatisticId = u.statistic_id,
                Role = u.role != null ? new RoleDto
                {
                    RoleId = u.role.role_id,
                    RoleName = u.role.role_name
                } : null
            }).ToList()
        };
    }
    
    public async Task<ClassDto> GetClassWithUsersByIdAsync(int id)
    {
        // Sử dụng phương thức mới của Repository để tải users
        var cls = await _unitOfWork.Classes.GetClassWithUsersAsync(id);
        if (cls == null)
        {
            throw new NotFoundException("Class", "Id", id);
        }
        return MapToClassDto(cls); // Gọi MapToClassDto để chuyển đổi
    }
    
    
    public async Task<IEnumerable<UserDto>> GetAvailableStudentsAndTeachersAsync()
    {
        // Lấy Role ID cho "Student" và "Teacher"
        var studentRole = await _unitOfWork.Roles.FindOneAsync(r => r.role_name == "Student");
        var teacherRole = await _unitOfWork.Roles.FindOneAsync(r => r.role_name == "Teacher");

        var roleNames = new List<string>();
        if (studentRole != null) roleNames.Add(studentRole.role_name);
        if (teacherRole != null) roleNames.Add(teacherRole.role_name);

        if (!roleNames.Any())
        {
            throw new NotFoundException("Roles", "Names", "Student or Teacher. Please ensure these roles exist in the database with these exact names.");
        }

        var users = await _unitOfWork.Users.GetUsersByRoleNamesAsync(roleNames);

        // Ánh xạ sang UserDto. UserDto giờ đã có thuộc tính RoleDto.
        return users.Select(u => new UserDto
        {
            UserId = u.user_id,
            Username = u.username,
            AccountName = u.account_name,
            Address = u.address,
            PhoneNumber = u.phone_number,
            IsDisabled = u.is_disabled,
            CreateAt = u.create_at,
            AvatarUrl = u.avatar_url,
            Birthday = u.birthday,
            RoleId = u.role_id,
            StatisticId = u.statistic_id,
            Role = u.role != null ? new RoleDto
            {
                RoleId = u.role.role_id,
                RoleName = u.role.role_name
            } : null
        });
    }

    public async Task AssignUsersToClassAsync(int classId, List<int> userIds)
    {
        // Lấy class bao gồm cả danh sách người dùng hiện tại
        var targetClass = await _unitOfWork.Classes.GetById(classId); // ClassRepository.GetById đã include users
        if (targetClass == null)
        {
            throw new NotFoundException("Class", "Id", classId);
        }

        // Lấy Role ID cho "Student" và "Teacher" để xác thực
        var studentRole = await _unitOfWork.Roles.FindOneAsync(r => r.role_name == "Student");
        var teacherRole = await _unitOfWork.Roles.FindOneAsync(r => r.role_name == "Teacher");

        var validRoleIds = new List<int>();
        if (studentRole != null) validRoleIds.Add(studentRole.role_id);
        if (teacherRole != null) validRoleIds.Add(teacherRole.role_id);

        if (!validRoleIds.Any())
        {
            throw new ApiException("Student or Teacher roles not found in the system.", null, (int)HttpStatusCode.PreconditionFailed);
        }

        // Tạo danh sách người dùng mới cho lớp
        var newUsersForClass = new List<user>();
        foreach (var userId in userIds)
        {
            var user = await _unitOfWork.Users.GetUserWithRoleAsync(userId); // Đảm bảo phương thức này tải cả role
            if (user == null)
            {
                throw new NotFoundException("User", "Id", userId);
            }
            // Kiểm tra xem người dùng có vai trò hợp lệ không
            if (!user.role_id.HasValue || !validRoleIds.Contains(user.role_id.Value))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Users", new string[] { $"Người dùng có ID {userId} không phải là Học viên hoặc Giáo viên hợp lệ." } }
                });
            }
            newUsersForClass.Add(user);
        }

        // Xóa tất cả người dùng hiện có và thêm những người dùng mới
        targetClass.users.Clear();
        foreach (var user in newUsersForClass)
        {
            targetClass.users.Add(user);
        }

        try
        {
            // Entity Framework Core sẽ xử lý việc cập nhật bảng nối (join table)
            await _unitOfWork.Classes.UpdateAsync(targetClass);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi gán người dùng vào lớp học trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("Một lỗi không mong muốn đã xảy ra khi gán người dùng vào lớp học.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task AddUsersToClassAsync(int classId, List<int> userIds)
    {
        var targetClass = await _unitOfWork.Classes.GetById(classId);
        if (targetClass == null)
        {
            throw new NotFoundException("Class", "Id", classId);
        }

        var studentRole = await _unitOfWork.Roles.FindOneAsync(r => r.role_name == "Student");
        var teacherRole = await _unitOfWork.Roles.FindOneAsync(r => r.role_name == "Teacher");

        var validRoleIds = new List<int>();
        if (studentRole != null) validRoleIds.Add(studentRole.role_id);
        if (teacherRole != null) validRoleIds.Add(teacherRole.role_id);

        if (!validRoleIds.Any())
        {
            throw new ApiException("Student or Teacher roles not found in the system.", null, (int)HttpStatusCode.PreconditionFailed);
        }

        foreach (var userId in userIds)
        {
            // Kiểm tra xem người dùng đã có trong lớp chưa để tránh trùng lặp
            if (targetClass.users.Any(u => u.user_id == userId))
            {
                continue; // Bỏ qua nếu người dùng đã có trong lớp
            }

            var userToAdd = await _unitOfWork.Users.GetUserWithRoleAsync(userId);
            if (userToAdd == null)
            {
                throw new NotFoundException("User", "Id", userId);
            }
            // Kiểm tra vai trò hợp lệ
            if (!userToAdd.role_id.HasValue || !validRoleIds.Contains(userToAdd.role_id.Value))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Users", new string[] { $"Người dùng có ID {userId} không phải là Học viên hoặc Giáo viên hợp lệ." } }
                });
            }
            targetClass.users.Add(userToAdd);
        }

        try
        {
            await _unitOfWork.Classes.UpdateAsync(targetClass);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm người dùng vào lớp học trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("Một lỗi không mong muốn đã xảy ra khi thêm người dùng vào lớp học.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task RemoveUsersFromClassAsync(int classId, List<int> userIds)
    {
        var targetClass = await _unitOfWork.Classes.GetById(classId);
        if (targetClass == null)
        {
            throw new NotFoundException("Class", "Id", classId);
        }

        foreach (var userIdToRemove in userIds)
        {
            var userToRemove = targetClass.users.FirstOrDefault(u => u.user_id == userIdToRemove);
            if (userToRemove != null)
            {
                targetClass.users.Remove(userToRemove); // Xóa khỏi collection
            }
            // Tùy chọn: ném lỗi nếu người dùng không được tìm thấy trong lớp
            // else { throw new NotFoundException("User in Class", "Id", userIdToRemove); }
        }

        try
        {
            await _unitOfWork.Classes.UpdateAsync(targetClass);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi xóa người dùng khỏi lớp học trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("Một lỗi không mong muốn đã xảy ra khi xóa người dùng khỏi lớp học.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }
}