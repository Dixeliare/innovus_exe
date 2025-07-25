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
        // Sử dụng AnyAsync để kiểm tra tồn tại hiệu quả hơn
        var classExists = await _unitOfWork.Classes.AnyAsync(c => c.class_id == classId);
        if (!classExists)
        {
            throw new NotFoundException("Class", "Id", classId);
        }

        var sessions = await _unitOfWork.ClassSessions.GetClassSessionsByClassIdWithDetailsAsync(classId);
        return sessions.Select(MapToPersonalClassSessionDto);
    }

    public async Task<IEnumerable<PersonalClassSessionDto>> GetClassSessionsByDayIdAsync(int dayId)
    {
        // Sử dụng AnyAsync để kiểm tra tồn tại hiệu quả hơn
        var dayExists = await _unitOfWork.Days.AnyAsync(d => d.day_id == dayId);
        if (!dayExists)
        {
            throw new NotFoundException("Day", "Id", dayId);
        }

        var sessions = await _unitOfWork.ClassSessions.GetClassSessionsByDayIdWithDetailsAsync(dayId);
        return sessions.Select(MapToPersonalClassSessionDto);
    }

    public async Task<BaseClassSessionDto> AddAsync(CreateClassSessionDto createClassSessionDto)
    {
        // 1. Basic validation (Data Annotations on DTO handle some, manual for others)
        if (createClassSessionDto.SessionNumber.HasValue && createClassSessionDto.SessionNumber.Value <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "SessionNumber", new string[] { "Số buổi học phải là số dương nếu được cung cấp." } }
            });
        }
        // RoomCode không còn ở đây, đã thay bằng RoomId (được Required)

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

        // ĐÃ SỬA: Thêm kiểm tra RoomId
        var roomExists = await _unitOfWork.Rooms.GetByIdAsync(createClassSessionDto.RoomId);
        if (roomExists == null)
        {
            throw new NotFoundException("Room", "Id", createClassSessionDto.RoomId);
        }

        // 3. Check for uniqueness (same Day, Class, TimeSlot combination)
        var existingSession = await _unitOfWork.ClassSessions.SearchClassSessionsAsync(
            classId: createClassSessionDto.ClassId,
            dayId: createClassSessionDto.DayId,
            timeSlotId: createClassSessionDto.TimeSlotId
            // Không cần RoomId trong kiểm tra duy nhất nếu ràng buộc chỉ là Day, Class, TimeSlot
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
            date = createClassSessionDto.Date,
            room_id = createClassSessionDto.RoomId, // ĐÃ SỬA: Gán room_id từ DTO
            day_id = createClassSessionDto.DayId,
            class_id = createClassSessionDto.ClassId,
            time_slot_id = createClassSessionDto.TimeSlotId
        };

        try
        {
            var addedSession = await _unitOfWork.ClassSessions.AddAsync(sessionEntity);
            await _unitOfWork.CompleteAsync();
            // Để trả về đầy đủ thông tin RoomCode, cần fetch lại với details
            var addedSessionWithDetails = await _unitOfWork.ClassSessions.GetClassSessionByIdWithDetailsAsync(addedSession.class_session_id);
            return MapToBaseClassSessionDto(addedSessionWithDetails ?? addedSession);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            // Logging the full exception details is recommended for debugging
            // _logger.LogError(dbEx, "DbUpdateException during ClassSession AddAsync.");
            throw new ApiException("Có lỗi xảy ra khi lưu buổi học vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "An unexpected error occurred during class session creation.");
            throw new ApiException("Đã xảy ra lỗi không mong muốn khi tạo buổi học.", ex, (int)HttpStatusCode.InternalServerError);
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

        // ĐÃ SỬA: Xử lý RoomId thay vì RoomCode
        bool foreignKeyChanged = false; // Biến cờ này phải được quản lý cẩn thận

        if (updateClassSessionDto.RoomId.HasValue && existingSession.room_id != updateClassSessionDto.RoomId.Value)
        {
            var roomExists = await _unitOfWork.Rooms.GetByIdAsync(updateClassSessionDto.RoomId.Value);
            if (roomExists == null)
            {
                throw new NotFoundException("Room", "Id", updateClassSessionDto.RoomId.Value);
            }
            existingSession.room_id = updateClassSessionDto.RoomId.Value;
            foreignKeyChanged = true;
        }

        // Check and update other foreign keys
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
        if (foreignKeyChanged || 
            (updateClassSessionDto.DayId.HasValue || updateClassSessionDto.ClassId.HasValue || updateClassSessionDto.TimeSlotId.HasValue)) // Kiểm tra các trường có thể thay đổi để kích hoạt kiểm tra
        {
            var targetClassId = updateClassSessionDto.ClassId ?? existingSession.class_id;
            var targetDayId = updateClassSessionDto.DayId ?? existingSession.day_id;
            var targetTimeSlotId = updateClassSessionDto.TimeSlotId ?? existingSession.time_slot_id;

            var existingSessionConflict = await _unitOfWork.ClassSessions.SearchClassSessionsAsync(
                classId: targetClassId,
                dayId: targetDayId,
                timeSlotId: targetTimeSlotId
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
            // _logger.LogError(dbEx, "DbUpdateException during ClassSession UpdateAsync.");
            throw new ApiException("Có lỗi xảy ra khi cập nhật buổi học trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "An unexpected error occurred during class session update.");
            throw new ApiException("Đã xảy ra lỗi không mong muốn khi cập nhật buổi học.", ex, (int)HttpStatusCode.InternalServerError);
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
        var relatedAttendances = await _unitOfWork.Attendances.GetAttendancesByClassSessionIdAsync(id);
        if (relatedAttendances != null && relatedAttendances.Any())
        {
            throw new ApiException($"Không thể xóa buổi học với ID {id} vì có các bản ghi điểm danh liên quan.", (int)HttpStatusCode.Conflict);
        }

        try
        {
            var result = await _unitOfWork.ClassSessions.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            return result;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            // _logger.LogError(dbEx, $"DbUpdateException during ClassSession DeleteAsync for ID {id}.");
            throw new ApiException("Có lỗi xảy ra khi xóa buổi học khỏi cơ sở dữ liệu. Có thể có các bản ghi liên quan.", dbEx, (int)HttpStatusCode.Conflict);
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "An unexpected error occurred during class session deletion.");
            throw new ApiException("Đã xảy ra lỗi không mong muốn khi xóa buổi học.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // ĐÃ SỬA: Thay đổi tham số roomCode thành roomId
    public async Task<IEnumerable<PersonalClassSessionDto>> SearchClassSessionsAsync(
        int? sessionNumber = null,
        DateOnly? date = null,
        int? roomId = null, // ĐÃ SỬA: Thay đổi từ string? roomCode sang int? roomId
        int? classId = null,
        int? dayId = null,
        int? timeSlotId = null)
    {
        var sessions = await _unitOfWork.ClassSessions.SearchClassSessionsWithDetailsAsync(
            sessionNumber, date, roomId, classId, dayId, timeSlotId // ĐÃ SỬA: Truyền roomId
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
            RoomCode = model.room?.room_code, // ĐÃ SỬA: Truy cập room_code qua navigation property 'room'
            
            DayId = model.day_id, // Giữ nguyên, đã đúng là int
            ClassId = model.class_id, // Giữ nguyên, đã đúng là int
            TimeSlotId = model.time_slot_id, // Giữ nguyên, đã đúng là int
            
            // Details from Day
            WeekId = model.day?.week_id, // ĐÃ THÊM: Nếu bạn muốn WeekId trong DTO
            DateOfDay = model.day?.date_of_day,
            DayOfWeekName = model.day?.day_of_week_name,

            // Details from Week (accessible via model.day.week)
            WeekNumberInMonth = model.day?.week?.week_number_in_month,

            // Details from Class
            ClassCode = model._class?.class_code,
            InstrumentName = model._class?.instrument?.instrument_name,

            // Details from TimeSlot
            StartTime = model.time_slot?.start_time.ToTimeSpan(),
            EndTime = model.time_slot?.end_time.ToTimeSpan()
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
            RoomCode = model.room?.room_code, // ĐÃ SỬA: Truy cập room_code qua navigation property 'room'
            DayId = model.day_id,
            ClassId = model.class_id,
            TimeSlotId = model.time_slot_id,
        };
    }
}