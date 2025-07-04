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

public class ClassSessionService : IClassSessionService
{
    // private readonly IClassSessionRepository _classSessionRepository;
    // private readonly IWeekRepository _weekRepository; // Inject cho kiểm tra khóa ngoại
    // private readonly IClassRepository _classRepository; // Inject cho kiểm tra khóa ngoại
    // private readonly ITimeslotRepository _timeSlotRepository; // Inject cho kiểm tra khóa ngoại
    //
    // public ClassSessionService(IClassSessionRepository classSessionRepository,
    //     IWeekRepository weekRepository,
    //     IClassRepository classRepository,
    //     ITimeslotRepository timeSlotRepository)
    // {
    //     _classSessionRepository = classSessionRepository;
    //     _weekRepository = weekRepository;
    //     _classRepository = classRepository;
    //     _timeSlotRepository = timeSlotRepository;
    // }
    
    private readonly IUnitOfWork _unitOfWork;

    public ClassSessionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ClassSessionDto>> GetAllAsync()
    {
        var classSessions = await _unitOfWork.ClassSessions.GetAllAsync();
        return classSessions.Select(MapToClassSessionDto);
    }

    public async Task<ClassSessionDto> GetByIdAsync(int id)
    {
        var classSession = await _unitOfWork.ClassSessions.GetByIdAsync(id);
        if (classSession == null)
        {
            throw new NotFoundException("ClassSession", "Id", id);
        }
        return MapToClassSessionDto(classSession);
    }

    public async Task<ClassSessionDto> AddAsync(CreateClassSessionDto createClassSessionDto)
    {
        // Kiểm tra sự tồn tại của các khóa ngoại
        var weekExists = await _unitOfWork.Weeks.GetByIdAsync(createClassSessionDto.WeekId);
        if (weekExists == null)
        {
            throw new NotFoundException("Week", "Id", createClassSessionDto.WeekId);
        }

        var classExists = await _unitOfWork.Classes.GetByIdAsync(createClassSessionDto.ClassId);
        if (classExists == null)
        {
            throw new NotFoundException("Class", "Id", createClassSessionDto.ClassId);
        }

        var timeSlotExists = await _unitOfWork.Timeslots.GetByIdAsync(createClassSessionDto.TimeSlotId);
        if (timeSlotExists == null)
        {
            throw new NotFoundException("TimeSlot", "Id", createClassSessionDto.TimeSlotId);
        }

        // Kiểm tra tính duy nhất: Một lớp học chỉ có thể có một buổi học vào một ngày và khung giờ cụ thể
        var existingSession = await _unitOfWork.ClassSessions.FindOneAsync(cs =>
            cs.date == createClassSessionDto.Date &&
            cs.class_id == createClassSessionDto.ClassId &&
            cs.time_slot_id == createClassSessionDto.TimeSlotId);

        if (existingSession != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Conflict", new string[] { $"Buổi học cho lớp ID '{createClassSessionDto.ClassId}' vào ngày '{createClassSessionDto.Date:dd/MM/yyyy}' và khung giờ ID '{createClassSessionDto.TimeSlotId}' đã tồn tại." } }
            });
        }

        var classSessionEntity = new class_session
        {
            session_number = createClassSessionDto.SessionNumber,
            date = createClassSessionDto.Date,
            room_code = createClassSessionDto.RoomCode,
            week_id = createClassSessionDto.WeekId,
            class_id = createClassSessionDto.ClassId,
            time_slot_id = createClassSessionDto.TimeSlotId
        };

        try
        {
            var addedClassSession = await _unitOfWork.ClassSessions.AddAsync(classSessionEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
            return MapToClassSessionDto(addedClassSession);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm buổi học vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the class session.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateClassSessionDto updateClassSessionDto)
    {
        var existingClassSession = await _unitOfWork.ClassSessions.GetByIdAsync(updateClassSessionDto.ClassSessionId);

        if (existingClassSession == null)
        {
            throw new NotFoundException("ClassSession", "Id", updateClassSessionDto.ClassSessionId);
        }

        // Kiểm tra và cập nhật khóa ngoại WeekId
        if (updateClassSessionDto.WeekId.HasValue && updateClassSessionDto.WeekId.Value != existingClassSession.week_id)
        {
            var weekExists = await _unitOfWork.Weeks.GetByIdAsync(updateClassSessionDto.WeekId.Value);
            if (weekExists == null)
            {
                throw new NotFoundException("Week", "Id", updateClassSessionDto.WeekId.Value);
            }
            existingClassSession.week_id = updateClassSessionDto.WeekId.Value;
        }

        // Kiểm tra và cập nhật khóa ngoại ClassId
        if (updateClassSessionDto.ClassId.HasValue && updateClassSessionDto.ClassId.Value != existingClassSession.class_id)
        {
            var classExists = await _unitOfWork.Classes.GetByIdAsync(updateClassSessionDto.ClassId.Value);
            if (classExists == null)
            {
                throw new NotFoundException("Class", "Id", updateClassSessionDto.ClassId.Value);
            }
            existingClassSession.class_id = updateClassSessionDto.ClassId.Value;
        }

        // Kiểm tra và cập nhật khóa ngoại TimeSlotId
        if (updateClassSessionDto.TimeSlotId.HasValue && updateClassSessionDto.TimeSlotId.Value != existingClassSession.time_slot_id)
        {
            var timeSlotExists = await _unitOfWork.Timeslots.GetByIdAsync(updateClassSessionDto.TimeSlotId.Value);
            if (timeSlotExists == null)
            {
                throw new NotFoundException("TimeSlot", "Id", updateClassSessionDto.TimeSlotId.Value);
            }
            existingClassSession.time_slot_id = updateClassSessionDto.TimeSlotId.Value;
        }

        // Kiểm tra tính duy nhất: Một lớp học chỉ có thể có một buổi học vào một ngày và khung giờ cụ thể
        // Cần kiểm tra xem sự kết hợp mới có trùng với buổi học khác (không phải chính nó) không
        if (updateClassSessionDto.Date.HasValue && updateClassSessionDto.Date.Value != existingClassSession.date ||
            updateClassSessionDto.ClassId.HasValue && updateClassSessionDto.ClassId.Value != existingClassSession.class_id ||
            updateClassSessionDto.TimeSlotId.HasValue && updateClassSessionDto.TimeSlotId.Value != existingClassSession.time_slot_id)
        {
            DateOnly targetDate = updateClassSessionDto.Date ?? existingClassSession.date ?? default;
            int targetClassId = updateClassSessionDto.ClassId ?? existingClassSession.class_id;
            int targetTimeSlotId = updateClassSessionDto.TimeSlotId ?? existingClassSession.time_slot_id;

            var conflictingSession = await _unitOfWork.ClassSessions.FindOneAsync(cs =>
                cs.date == targetDate &&
                cs.class_id == targetClassId &&
                cs.time_slot_id == targetTimeSlotId &&
                cs.class_session_id != updateClassSessionDto.ClassSessionId); // Không phải chính nó

            if (conflictingSession != null)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Conflict", new string[] { $"Buổi học cho lớp ID '{targetClassId}' vào ngày '{targetDate:dd/MM/yyyy}' và khung giờ ID '{targetTimeSlotId}' đã tồn tại với ID khác." } }
                });
            }
        }


        // Cập nhật các trường còn lại
        existingClassSession.session_number = updateClassSessionDto.SessionNumber ?? existingClassSession.session_number;
        existingClassSession.date = updateClassSessionDto.Date ?? existingClassSession.date;
        existingClassSession.room_code = updateClassSessionDto.RoomCode ?? existingClassSession.room_code;


        try
        {
            await _unitOfWork.ClassSessions.UpdateAsync(existingClassSession);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật buổi học trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the class session.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var classSessionToDelete = await _unitOfWork.ClassSessions.GetByIdAsync(id);
        if (classSessionToDelete == null)
        {
            throw new NotFoundException("ClassSession", "Id", id);
        }

        try
        {
            // Kiểm tra xem có bất kỳ Attendance nào liên quan đến buổi học này không
            var hasRelatedAttendances = await _unitOfWork.Attendances.AnyAsync(a => a.class_session_id == id);
            if (hasRelatedAttendances)
            {
                throw new ApiException("Không thể xóa buổi học này vì có dữ liệu điểm danh liên quan.", null, (int)HttpStatusCode.Conflict); // 409 Conflict
            }

            await _unitOfWork.ClassSessions.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi xóa buổi học khỏi cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the class session.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<ClassSessionDto>> SearchClassSessionsAsync(DateOnly? date = null, string? roomCode = null, int? weekId = null, int? classId = null, int? timeSlotId = null)
    {
        var classSessions = await _unitOfWork.ClassSessions.SearchClassSessionsAsync(date, roomCode, weekId, classId, timeSlotId); // Giả định SearchClassSessionsAsync có sẵn
        return classSessions.Select(MapToClassSessionDto);
    }
    
    private ClassSessionDto MapToClassSessionDto(class_session model)
    {
        return new ClassSessionDto
        {
            ClassSessionId = model.class_session_id,
            SessionNumber = model.session_number,
            Date = model.date,
            RoomCode = model.room_code,
            WeekId = model.week_id,
            ClassId = model.class_id,
            TimeSlotId = model.time_slot_id
            // Nếu bạn có DTO lồng nhau, bạn sẽ map ở đây:
            // Class = model._class != null ? new ClassDto { ClassId = model._class.class_id, Name = model._class.name } : null
            // ...
        };
    }
}