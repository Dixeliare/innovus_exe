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

        // THÊM LOGIC KIỂM TRA INSTRUMENTID
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

        var scheduleEntity = new opening_schedule
        {
            class_code = createOpeningScheduleDto.ClassCode,
            opening_day = createOpeningScheduleDto.OpeningDay,
            end_date = createOpeningScheduleDto.EndDate,
            schedule = createOpeningScheduleDto.Schedule,
            student_quantity = createOpeningScheduleDto.StudentQuantity,
            is_advanced_class = createOpeningScheduleDto.IsAdvancedClass ?? false,
            teacher_user_id = createOpeningScheduleDto.TeacherUserId,
            instrument_id = createOpeningScheduleDto.InstrumentId // GÁN GIÁ TRỊ INSTRUMENTID TỪ DTO
        };

        try
        {
            var addedSchedule = await _unitOfWork.OpeningSchedules.AddAsync(scheduleEntity);
            await _unitOfWork.CompleteAsync();

            // Tải lại schedule để có các navigation properties nếu cần cho DTO trả về
            // Đảm bảo GetByIdAsync trong repo đã include instrument
            var addedScheduleWithDetails =
                await _unitOfWork.OpeningSchedules.GetByIdAsync(addedSchedule.opening_schedule_id);

            return MapToOpeningScheduleDto(addedScheduleWithDetails ?? addedSchedule);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm lịch khai giảng vào cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the opening schedule.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    // Services/Services/OpeningScheduleService.cs

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
            await _unitOfWork.OpeningSchedules.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu có user nào đó đang liên kết với lịch khai giảng này, sẽ ném lỗi FK
            throw new ApiException(
                "Không thể xóa lịch khai giảng này vì nó đang được sử dụng bởi một hoặc nhiều người dùng.", dbEx,
                (int)HttpStatusCode.Conflict); // 409 Conflict
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
            // BỎ ÁNH XẠ class_codeNavigation (nếu đã xóa từ DTO)
            // ClassNavigation = null, // hoặc xóa hẳn dòng này

            // THÊM ÁNH XẠ INSTRUMENTID VÀ INSTRUMENT OBJECT
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