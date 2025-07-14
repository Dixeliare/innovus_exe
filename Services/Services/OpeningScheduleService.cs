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

public class OpeningScheduleService : IOpeningScheduleService
{
    // private readonly IOpeningScheduleRepository _openingScheduleRepository;
    //
    // public OpeningScheduleService(IOpeningScheduleRepository openingScheduleRepository) => _openingScheduleRepository = openingScheduleRepository;

    private readonly IUnitOfWork _unitOfWork;

    public OpeningScheduleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<OpeningScheduleDto>> GetAllAsync()
    {
        var schedules = await _unitOfWork.OpeningSchedules.GetAllAsync();
        return schedules.Select(MapToOpeningScheduleDto);
    }

    public async Task<OpeningScheduleDto> GetByIdAsync(int id)
    {
        var schedule = await _unitOfWork.OpeningSchedules.GetByIdAsync(id);
        if (schedule == null)
        {
            throw new NotFoundException("Opening Schedule", "Id", id);
        }

        return MapToOpeningScheduleDto(schedule);
    }

    public async Task<OpeningScheduleDto> AddAsync(CreateOpeningScheduleDto createOpeningScheduleDto)
    {
        // Kiểm tra ClassCode đã tồn tại trong OpeningSchedule
        var existingScheduleWithClassCode =
            await _unitOfWork.OpeningSchedules.FindOneAsync(os =>
                os.class_code == createOpeningScheduleDto.ClassCode);
        if (existingScheduleWithClassCode != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                {
                    "ClassCode",
                    new string[] { $"Mã lớp '{createOpeningScheduleDto.ClassCode}' đã có lịch khai giảng." }
                }
            });
        }

        // Kiểm tra logic: Ngày kết thúc không được trước ngày khai giảng
        if (createOpeningScheduleDto.EndDate.HasValue &&
            createOpeningScheduleDto.EndDate.Value < createOpeningScheduleDto.OpeningDay)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "EndDate", new string[] { "Ngày kết thúc không được trước ngày khai giảng." } }
            });
        }

        // Kiểm tra giáo viên và vai trò
        user? teacherUser = null;
        if (createOpeningScheduleDto.TeacherUserId.HasValue)
        {
            teacherUser = await _unitOfWork.Users.GetUserWithRoleAsync(createOpeningScheduleDto.TeacherUserId.Value);

            if (teacherUser == null)
            {
                throw new NotFoundException("Teacher User", "Id", createOpeningScheduleDto.TeacherUserId.Value);
            }

            if (teacherUser.role?.role_name?.ToLower() != "teacher")
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "TeacherUserId", new string[] { "Người dùng được chọn không phải là giáo viên." } }
                });
            }
        }

        // Kiểm tra InstrumentId
        if (createOpeningScheduleDto.InstrumentId <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "InstrumentId", new string[] { "Mã nhạc cụ không hợp lệ." } }
            });
        }

        var existingInstrument = await _unitOfWork.Instruments.GetByIdAsync(createOpeningScheduleDto.InstrumentId);
        if (existingInstrument == null)
        {
            throw new NotFoundException("Instrument", "Id", createOpeningScheduleDto.InstrumentId);
        }

        // --- Bắt đầu thay đổi ở đây ---

        // 1. Tạo entity opening_schedule
        var scheduleEntity = new opening_schedule
        {
            class_code = createOpeningScheduleDto.ClassCode,
            opening_day = createOpeningScheduleDto.OpeningDay,
            end_date = createOpeningScheduleDto.EndDate,
            schedule = createOpeningScheduleDto.Schedule,
            student_quantity = createOpeningScheduleDto.StudentQuantity,
            is_advanced_class = createOpeningScheduleDto.IsAdvancedClass ?? false,
            teacher_user_id = createOpeningScheduleDto.TeacherUserId,
            instrument_id = createOpeningScheduleDto.InstrumentId
        };

        try
        {
            // Thêm opening_schedule vào DbContext
            _unitOfWork.OpeningSchedules.AddAsync(scheduleEntity); // Gọi phương thức Add từ GenericRepository

            // 2. Tạo entity _class tự động với cùng class_code và instrument_id
            var classEntity = new _class
            {
                class_code = createOpeningScheduleDto.ClassCode,
                instrument_id = createOpeningScheduleDto.InstrumentId
            };
            
            // Thêm class vào DbContext
            _unitOfWork.Classes.AddAsync(classEntity); // Gọi phương thức Add từ GenericRepository

            // 3. Lưu cả hai thay đổi (opening_schedule và _class) trong một transaction
            await _unitOfWork.CompleteAsync(); 

            // Tải lại schedule để có các navigation properties nếu cần cho DTO trả về
            var addedScheduleWithDetails =
                await _unitOfWork.OpeningSchedules.GetByIdAsync(scheduleEntity.opening_schedule_id);

            return MapToOpeningScheduleDto(addedScheduleWithDetails ?? scheduleEntity);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm lịch khai giảng hoặc lớp học vào cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the opening schedule and class.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateOpeningScheduleDto updateOpeningScheduleDto)
    {
        var existingSchedule =
            await _unitOfWork.OpeningSchedules.GetByIdAsync(updateOpeningScheduleDto.OpeningScheduleId);

        if (existingSchedule == null)
        {
            throw new NotFoundException("Opening Schedule", "Id", updateOpeningScheduleDto.OpeningScheduleId);
        }

        // ... (Giữ nguyên logic kiểm tra giáo viên và vai trò) ...
        if (updateOpeningScheduleDto.TeacherUserId.HasValue &&
            updateOpeningScheduleDto.TeacherUserId.Value != existingSchedule.teacher_user_id)
        {
            var teacherUser =
                await _unitOfWork.Users.GetUserWithRoleAsync(updateOpeningScheduleDto.TeacherUserId.Value);
            if (teacherUser == null)
            {
                throw new NotFoundException("Teacher User", "Id", updateOpeningScheduleDto.TeacherUserId.Value);
            }

            if (teacherUser.role?.role_name?.ToLower() != "teacher")
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "TeacherUserId", new string[] { "Người dùng được chọn không phải là giáo viên." } }
                });
            }

            existingSchedule.teacher_user_id = updateOpeningScheduleDto.TeacherUserId.Value;
        }

        // THÊM LOGIC KIỂM TRA VÀ CẬP NHẬT INSTRUMENTID
        if (updateOpeningScheduleDto.InstrumentId.HasValue &&
            updateOpeningScheduleDto.InstrumentId.Value != existingSchedule.instrument_id)
        {
            var existingInstrument =
                await _unitOfWork.Instruments.GetByIdAsync(updateOpeningScheduleDto.InstrumentId.Value);
            if (existingInstrument == null)
            {
                throw new NotFoundException("Instrument", "Id", updateOpeningScheduleDto.InstrumentId.Value);
            }

            existingSchedule.instrument_id = updateOpeningScheduleDto.InstrumentId.Value;
        }

        // Cập nhật các trường nếu có giá trị được cung cấp
        existingSchedule.class_code = updateOpeningScheduleDto.ClassCode ?? existingSchedule.class_code;
        existingSchedule.opening_day = updateOpeningScheduleDto.OpeningDay ?? existingSchedule.opening_day;
        existingSchedule.end_date = updateOpeningScheduleDto.EndDate ?? existingSchedule.end_date;
        existingSchedule.schedule = updateOpeningScheduleDto.Schedule ?? existingSchedule.schedule;
        existingSchedule.student_quantity =
            updateOpeningScheduleDto.StudentQuantity ?? existingSchedule.student_quantity;
        existingSchedule.is_advanced_class =
            updateOpeningScheduleDto.IsAdvancedClass ?? existingSchedule.is_advanced_class;

        try
        {
            await _unitOfWork.OpeningSchedules.UpdateAsync(existingSchedule);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật lịch khai giảng trong cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the opening schedule.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var scheduleToDelete = await _unitOfWork.OpeningSchedules.GetByIdAsync(id);
        if (scheduleToDelete == null)
        {
            throw new NotFoundException("Opening Schedule", "Id", id);
        }

        try
        {
            // Kiểm tra xem có class nào liên kết với opening_schedule này không
            var relatedClass = await _unitOfWork.Classes.FindOneAsync(c => c.class_code == scheduleToDelete.class_code);

            if (relatedClass != null)
            {
                // Trước khi xóa opening_schedule, cần xóa hoặc điều chỉnh class liên quan.
                // Nếu muốn tự động xóa class khi xóa opening_schedule, bạn cần cấu hình Cascade Delete trong DbContext
                // Hoặc xóa class một cách tường minh ở đây.
                // Lưu ý: Nếu có học viên đang trong lớp này, việc xóa lớp có thể gặp lỗi ràng buộc khóa ngoại.
                // Cần xử lý các ràng buộc này (ví dụ: chuyển học viên sang lớp khác, hoặc xóa học viên khỏi lớp trước).
                
                // Để đơn giản, ở đây ta sẽ kiểm tra các ràng buộc:
                var hasRelatedSessions = await _unitOfWork.ClassSessions.AnyAsync(cs => cs.class_id == relatedClass.class_id);
                if (hasRelatedSessions)
                {
                    throw new ApiException("Không thể xóa lịch khai giảng này vì lớp học liên quan có các buổi học.", null, (int)HttpStatusCode.Conflict);
                }

                var hasRelatedUsersInClass = relatedClass.users.Any(); // Kiểm tra xem có người dùng nào liên kết với lớp không
                // Nếu GetById trong ClassRepository không include users, cần dùng FindOneAsync để kiểm tra user_class join table
                var hasUsersInJoinTable = await _unitOfWork.Users.AnyAsync(u => u.classes.Any(c => c.class_id == relatedClass.class_id));

                if (hasUsersInJoinTable)
                {
                    throw new ApiException("Không thể xóa lịch khai giảng này vì lớp học liên quan có người dùng (học viên/giáo viên) đang tham gia.", null, (int)HttpStatusCode.Conflict);
                }

                // Nếu không có ràng buộc, tiến hành xóa class trước
                await _unitOfWork.Classes.DeleteAsync(relatedClass.class_id);
            }

            // Sau khi xử lý class liên quan, xóa opening_schedule
            await _unitOfWork.OpeningSchedules.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu có user nào đó đang liên kết với lịch khai giảng này, sẽ ném lỗi FK
            throw new ApiException(
                "Có lỗi xảy ra khi xóa lịch khai giảng khỏi cơ sở dữ liệu. Vui lòng kiểm tra các ràng buộc liên quan.", dbEx,
                (int)HttpStatusCode.InternalServerError); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the opening schedule.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<OpeningScheduleDto>> SearchOpeningSchedulesAsync(
        string? classCode = null, DateOnly? openingDay = null,
        DateOnly? endDate = null, string? schedule = null, int? studentQuantity = null, bool? isAdvancedClass = null)
    {
        var schedules = await _unitOfWork.OpeningSchedules.SearchOpeningSchedulesAsync(classCode, openingDay, endDate, schedule, studentQuantity, isAdvancedClass);
        // Map kết quả tìm kiếm sang DTO
        return schedules.Select(MapToOpeningScheduleDto);
    }

    private OpeningScheduleDto MapToOpeningScheduleDto(opening_schedule model)
    {
        return new OpeningScheduleDto
        {
            OpeningScheduleId = model.opening_schedule_id,
            ClassCode = model.class_code,
            OpeningDay = model.opening_day,
            EndDate = model.end_date,
            Schedule = model.schedule,
            StudentQuantity = model.student_quantity,
            IsAdvancedClass = model.is_advanced_class,
            TeacherUser = model.teacher_user != null
                ? new UserForOpeningScheduleDto
                {
                    AccountName = model.teacher_user.account_name
                }
                : null,
            InstrumentId = model.instrument_id,
            Instrument = model.instrument != null
                ? new InstrumentDto
                {
                    InstrumentId = model.instrument.instrument_id,
                    InstrumentName = model.instrument.instrument_name
                }
                : null
        };
    }
}