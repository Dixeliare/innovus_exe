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
    private readonly IUnitOfWork _unitOfWork;

    public ClassSessionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<PersonalClassSessionDto>> GetAllAsync()
    {
        var sessions = await _unitOfWork.ClassSessions.GetAllClassSessionsWithDetailsAsync();
        return sessions.Select(MapToPersonalClassSessionDto);
    }

    public async Task<PersonalClassSessionDto> GetByIdAsync(int id)
    {
        var session = await _unitOfWork.ClassSessions.GetClassSessionByIdWithDetailsAsync(id);
        if (session == null)
        {
            throw new NotFoundException("ClassSession", "Id", id);
        }
        return MapToPersonalClassSessionDto(session);
    }

    public async Task<IEnumerable<PersonalClassSessionDto>> GetClassSessionsByClassIdAsync(int classId)
    {
        var classExists = await _unitOfWork.Classes.GetById(classId); // Có thể dùng AnyAsync để kiểm tra tồn tại nhanh hơn
        if (classExists == null)
        {
            throw new NotFoundException("Class", "Id", classId);
        }

        var sessions = await _unitOfWork.ClassSessions.GetClassSessionsByClassIdWithDetailsAsync(classId);
        return sessions.Select(MapToPersonalClassSessionDto);
    }

    public async Task<IEnumerable<PersonalClassSessionDto>> GetClassSessionsByDayIdAsync(int dayId)
    {
        var dayExists = await _unitOfWork.Days.GetByIdAsync(dayId); // Có thể dùng AnyAsync để kiểm tra tồn tại nhanh hơn
        if (dayExists == null)
        {
            throw new NotFoundException("Day", "Id", dayId);
        }

        var sessions = await _unitOfWork.ClassSessions.GetClassSessionsByDayIdWithDetailsAsync(dayId);
        return sessions.Select(MapToPersonalClassSessionDto);
    }

    public async Task<BaseClassSessionDto> AddAsync(CreateClassSessionDto createClassSessionDto)
    {
        // 1. Basic validation (Data Annotations on DTO handle some, manual for others)
        // SessionNumber là nullable, kiểm tra nếu có giá trị thì phải dương
        if (createClassSessionDto.SessionNumber.HasValue && createClassSessionDto.SessionNumber.Value <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "SessionNumber", new string[] { "Số buổi học phải là số dương nếu được cung cấp." } }
            });
        }
        // RoomCode là required, đã có [Required] và [StringLength] trên DTO

        // 2. Check foreign keys
        var dayExists = await _unitOfWork.Days.GetByIdAsync(createClassSessionDto.DayId);
        if (dayExists == null)
        {
            throw new NotFoundException("Day", "Id", createClassSessionDto.DayId);
        }

        var classExists = await _unitOfWork.Classes.GetById(createClassSessionDto.ClassId);
        if (classExists == null)
        {
            throw new NotFoundException("Class", "Id", createClassSessionDto.ClassId);
        }

        var timeSlotExists = await _unitOfWork.Timeslots.GetByIdAsync(createClassSessionDto.TimeSlotId);
        if (timeSlotExists == null)
        {
            throw new NotFoundException("TimeSlot", "Id", createClassSessionDto.TimeSlotId);
        }

        // 3. Check for uniqueness (same Day, Class, TimeSlot combination)
        var existingSession = await _unitOfWork.ClassSessions.SearchClassSessionsAsync(
            classId: createClassSessionDto.ClassId,
            dayId: createClassSessionDto.DayId,
            timeSlotId: createClassSessionDto.TimeSlotId
        );
        if (existingSession.Any())
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "DuplicateSession", new string[] { "Một buổi học đã tồn tại cho lớp này vào ngày và khung giờ này." } }
            });
        }

        var sessionEntity = new class_session
        {
            session_number = createClassSessionDto.SessionNumber,
            date = createClassSessionDto.Date, // Date là nullable
            room_code = createClassSessionDto.RoomCode,
            day_id = createClassSessionDto.DayId,
            class_id = createClassSessionDto.ClassId,
            time_slot_id = createClassSessionDto.TimeSlotId
        };

        try
        {
            var addedSession = await _unitOfWork.ClassSessions.AddAsync(sessionEntity);
            await _unitOfWork.CompleteAsync();
            // Return BaseClassSessionDto after creation, as PersonalClassSessionDto requires eager loading.
            // Caller can then GetById to get the full PersonalClassSessionDto.
            return MapToBaseClassSessionDto(addedSession);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            throw new ApiException("An error occurred while saving the class session to the database.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred during class session creation.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateClassSessionDto updateClassSessionDto)
    {
        if (updateClassSessionDto.ClassSessionId <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "ClassSessionId", new string[] { "ID buổi học không hợp lệ." } }
            });
        }

        var existingSession = await _unitOfWork.ClassSessions.GetByIdAsync(updateClassSessionDto.ClassSessionId);
        if (existingSession == null)
        {
            throw new NotFoundException("ClassSession", "Id", updateClassSessionDto.ClassSessionId);
        }

        // Update fields if provided
        if (updateClassSessionDto.SessionNumber.HasValue)
        {
            if (updateClassSessionDto.SessionNumber.Value <= 0)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "SessionNumber", new string[] { "Số buổi học phải là số dương nếu được cung cấp." } }
                });
            }
            existingSession.session_number = updateClassSessionDto.SessionNumber.Value;
        }
        if (updateClassSessionDto.Date.HasValue)
        {
            existingSession.date = updateClassSessionDto.Date.Value;
        }
        if (updateClassSessionDto.RoomCode != null) // RoomCode có thể là empty string
        {
            if (string.IsNullOrWhiteSpace(updateClassSessionDto.RoomCode))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "RoomCode", new string[] { "Mã phòng không được để trống." } }
                });
            }
            existingSession.room_code = updateClassSessionDto.RoomCode;
        }

        // Check and update foreign keys
        bool foreignKeyChanged = false;
        if (updateClassSessionDto.DayId.HasValue && existingSession.day_id != updateClassSessionDto.DayId.Value)
        {
            var dayExists = await _unitOfWork.Days.GetByIdAsync(updateClassSessionDto.DayId.Value);
            if (dayExists == null)
            {
                throw new NotFoundException("Day", "Id", updateClassSessionDto.DayId.Value);
            }
            existingSession.day_id = updateClassSessionDto.DayId.Value;
            foreignKeyChanged = true;
        }
        if (updateClassSessionDto.ClassId.HasValue && existingSession.class_id != updateClassSessionDto.ClassId.Value)
        {
            var classExists = await _unitOfWork.Classes.GetById(updateClassSessionDto.ClassId.Value);
            if (classExists == null)
            {
                throw new NotFoundException("Class", "Id", updateClassSessionDto.ClassId.Value);
            }
            existingSession.class_id = updateClassSessionDto.ClassId.Value;
            foreignKeyChanged = true;
        }
        if (updateClassSessionDto.TimeSlotId.HasValue && existingSession.time_slot_id != updateClassSessionDto.TimeSlotId.Value)
        {
            var timeSlotExists = await _unitOfWork.Timeslots.GetByIdAsync(updateClassSessionDto.TimeSlotId.Value);
            if (timeSlotExists == null)
            {
                throw new NotFoundException("TimeSlot", "Id", updateClassSessionDto.TimeSlotId.Value);
            }
            existingSession.time_slot_id = updateClassSessionDto.TimeSlotId.Value;
            foreignKeyChanged = true;
        }
        
        // If any foreign key changed or relevant fields changed, re-check uniqueness constraint
        // This check ensures that the updated combination (DayId, ClassId, TimeSlotId) doesn't already exist for *another* session.
        if (foreignKeyChanged || 
            (updateClassSessionDto.DayId.HasValue && updateClassSessionDto.ClassId.HasValue && updateClassSessionDto.TimeSlotId.HasValue))
        {
            var existingSessionConflict = await _unitOfWork.ClassSessions.SearchClassSessionsAsync(
                classId: updateClassSessionDto.ClassId ?? existingSession.class_id,
                dayId: updateClassSessionDto.DayId ?? existingSession.day_id,
                timeSlotId: updateClassSessionDto.TimeSlotId ?? existingSession.time_slot_id
            );
            // If another session with the exact same combination exists, and it's not the current session being updated
            if (existingSessionConflict.Any(s => s.class_session_id != existingSession.class_session_id))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "DuplicateSession", new string[] { "Một buổi học đã tồn tại cho lớp này vào ngày và khung giờ này sau khi cập nhật." } }
                });
            }
        }

        try
        {
            await _unitOfWork.ClassSessions.UpdateAsync(existingSession);
            await _unitOfWork.CompleteAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            throw new ApiException("An error occurred while updating the class session in the database.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred during class session update.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existingSession = await _unitOfWork.ClassSessions.GetByIdAsync(id);
        if (existingSession == null)
        {
            throw new NotFoundException("ClassSession", "Id", id);
        }
        
        // Check for related 'attendances' before deleting class_session
        // You'll need an IAttendanceRepository in your UnitOfWork
        var relatedAttendances = await _unitOfWork.Attendances.GetAttendancesByClassSessionIdAsync(id);
        if (relatedAttendances != null && relatedAttendances.Any())
        {
            throw new ApiException($"Cannot delete Class Session with ID {id} because it has related attendance records.", (int)HttpStatusCode.Conflict);
        }

        try
        {
            var result = await _unitOfWork.ClassSessions.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            return result;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            throw new ApiException("An error occurred while deleting the class session from the database. It might have related records.", dbEx, (int)HttpStatusCode.Conflict);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred during class session deletion.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<PersonalClassSessionDto>> SearchClassSessionsAsync(
        int? sessionNumber = null,
        DateOnly? date = null,
        string? roomCode = null,
        int? classId = null,
        int? dayId = null,
        int? timeSlotId = null)
    {
        var sessions = await _unitOfWork.ClassSessions.SearchClassSessionsWithDetailsAsync(
            sessionNumber, date, roomCode, classId, dayId, timeSlotId
        );
        return sessions.Select(MapToPersonalClassSessionDto);
    }

    // Map from Entity to PersonalClassSessionDto for detailed view
    private PersonalClassSessionDto MapToPersonalClassSessionDto(class_session model)
    {
        return new PersonalClassSessionDto
        {
            ClassSessionId = model.class_session_id,
            SessionNumber = model.session_number,
            Date = model.date,
            RoomCode = model.room_code,
            
            // Fix: ClassSession entity's day_id is non-nullable, so assign directly
            DayId = model.day_id, // <--- Sửa ở đây: gán trực tiếp day_id
            ClassId = model.class_id,
            TimeSlotId = model.time_slot_id,
            
            // Details from Day
            // Sử dụng toán tử ?. và ?? để xử lý nullable
            WeekId = model.day?.week_id, // int? = int?
            DateOfDay = model.day?.date_of_day, // DateOnly? = DateOnly?
            DayOfWeekName = model.day?.day_of_week_name, // string? = string?

            // Details from Week (accessible via model.day.week)
            // Cần đảm bảo rằng `model.day` và `model.day.week` đã được eager load trong repository
            WeekNumberInMonth = model.day?.week?.week_number_in_month, // int? = int?

            // Details from Class
            ClassCode = model._class?.class_code, // string? = string?
            // Assuming Class has Instrument navigation property
            InstrumentName = model._class?.instrument?.instrument_name, // string? = string?

            // Details from TimeSlot
            // time_slot?.start_time là TimeOnly? (nếu property trong entity là TimeOnly)
            // TimeOnly không thể tự động chuyển thành TimeSpan. Cần chuyển đổi thủ công nếu TimeSlot entity dùng TimeOnly.
            // Giả định rằng TimeSlot entity có StartTime/EndTime là TimeSpan hoặc bạn muốn chuyển đổi từ TimeOnly.
            // Nếu TimeSlot entity dùng TimeOnly:
            StartTime = model.time_slot?.start_time != null ? (TimeSpan?)model.time_slot.start_time.ToTimeSpan() : null, // <--- Sửa
            EndTime = model.time_slot?.end_time != null ? (TimeSpan?)model.time_slot.end_time.ToTimeSpan() : null // <--- Sửa
            // Nếu TimeSlot entity dùng TimeSpan:
            // StartTime = model.time_slot?.start_time,
            // EndTime = model.time_slot?.end_time
        };
    }

    // Map from Entity to BaseClassSessionDto for simple creation response
    private BaseClassSessionDto MapToBaseClassSessionDto(class_session model)
    {
        return new BaseClassSessionDto
        {
            ClassSessionId = model.class_session_id,
            SessionNumber = model.session_number,
            Date = model.date,
            RoomCode = model.room_code,
            DayId = model.day_id,
            ClassId = model.class_id,
            TimeSlotId = model.time_slot_id,
        };
    }
}