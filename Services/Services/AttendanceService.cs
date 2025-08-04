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

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AttendanceDto>> GetAllAsync()
    {
        var attendances = await _unitOfWork.Attendances.GetAllAttendancesWithDetailsAsync();
        return attendances.Select(MapToAttendanceDto);
    }

    public async Task<AttendanceDto> GetByIdAsync(int id)
    {
        var attendance = await _unitOfWork.Attendances.GetAttendanceByIdWithDetailsAsync(id);
        if (attendance == null)
        {
            throw new NotFoundException("Attendance", "Id", id);
        }
        return MapToAttendanceDto(attendance);
    }
    
    public async Task<IEnumerable<AttendanceDto>> GetAttendancesByUserIdAsync(int userId)
    {
        var userExists = await _unitOfWork.Users.GetByIdAsync(userId);
        if (userExists == null)
        {
            throw new NotFoundException("User", "Id", userId);
        }
        var attendances = await _unitOfWork.Attendances.SearchAttendancesWithDetailsAsync(userId: userId);
        return attendances.Select(MapToAttendanceDto);
    }

    public async Task<IEnumerable<AttendanceDto>> GetAttendancesByClassSessionIdAsync(int classSessionId)
    {
        var classSessionExists = await _unitOfWork.ClassSessions.GetByIdAsync(classSessionId);
        if (classSessionExists == null)
        {
            throw new NotFoundException("ClassSession", "Id", classSessionId);
        }
        var attendances = await _unitOfWork.Attendances.SearchAttendancesWithDetailsAsync(classSessionId: classSessionId);
        return attendances.Select(MapToAttendanceDto);
    }

    public async Task<AttendanceDto> AddAsync(CreateAttendanceDto createAttendanceDto)
    {
        var userExists = await _unitOfWork.Users.GetByIdAsync(createAttendanceDto.UserId);
        if (userExists == null)
        {
            throw new NotFoundException("User", "Id", createAttendanceDto.UserId);
        }

        var classSessionExists = await _unitOfWork.ClassSessions.GetByIdAsync(createAttendanceDto.ClassSessionId);
        if (classSessionExists == null)
        {
            throw new NotFoundException("ClassSession", "Id", createAttendanceDto.ClassSessionId);
        }

        var existingAttendances = await _unitOfWork.Attendances.SearchAttendancesAsync(
            userId: createAttendanceDto.UserId,
            classSessionId: createAttendanceDto.ClassSessionId
        );

        if (existingAttendances.Any())
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Conflict", new[] { $"Bản điểm danh cho User ID '{createAttendanceDto.UserId}' và Class Session ID '{createAttendanceDto.ClassSessionId}' đã tồn tại." } }
            });
        }

        var attendanceEntity = new attendance
        {
            status_id = createAttendanceDto.Status,
            check_at = DateTime.Now,
            note = createAttendanceDto.Note,
            user_id = createAttendanceDto.UserId,
            class_session_id = createAttendanceDto.ClassSessionId
        };

        try
        {
            var addedAttendance = await _unitOfWork.Attendances.AddAsync(attendanceEntity);
            await _unitOfWork.CompleteAsync();
            var addedAttendanceWithDetails = await _unitOfWork.Attendances.GetAttendanceByIdWithDetailsAsync(addedAttendance.attendance_id);
            return MapToAttendanceDto(addedAttendanceWithDetails ?? addedAttendance);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm bản điểm danh vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the attendance.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateAttendanceDto updateAttendanceDto)
    {
        var existingAttendance = await _unitOfWork.Attendances.GetByIdAsync(updateAttendanceDto.AttendanceId);

        if (existingAttendance == null)
        {
            throw new NotFoundException("Attendance", "Id", updateAttendanceDto.AttendanceId);
        }

        bool userIdChanged = false;
        if (updateAttendanceDto.UserId.HasValue && updateAttendanceDto.UserId.Value != existingAttendance.user_id)
        {
            var userExists = await _unitOfWork.Users.GetByIdAsync(updateAttendanceDto.UserId.Value);
            if (userExists == null)
            {
                throw new NotFoundException("User", "Id", updateAttendanceDto.UserId.Value);
            }
            existingAttendance.user_id = updateAttendanceDto.UserId.Value;
            userIdChanged = true;
        }

        bool classSessionIdChanged = false;
        if (updateAttendanceDto.ClassSessionId.HasValue && updateAttendanceDto.ClassSessionId.Value != existingAttendance.class_session_id)
        {
            var classSessionExists = await _unitOfWork.ClassSessions.GetByIdAsync(updateAttendanceDto.ClassSessionId.Value);
            if (classSessionExists == null)
            {
                throw new NotFoundException("ClassSession", "Id", updateAttendanceDto.ClassSessionId.Value);
            }
            existingAttendance.class_session_id = updateAttendanceDto.ClassSessionId.Value;
            classSessionIdChanged = true;
        }

        if (userIdChanged || classSessionIdChanged)
        {
            int targetUserId = updateAttendanceDto.UserId ?? existingAttendance.user_id;
            int targetClassSessionId = updateAttendanceDto.ClassSessionId ?? existingAttendance.class_session_id;

            var conflictingAttendances = await _unitOfWork.Attendances.SearchAttendancesAsync(
                userId: targetUserId,
                classSessionId: targetClassSessionId
            );

            if (conflictingAttendances.Any(att => att.attendance_id != updateAttendanceDto.AttendanceId))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Conflict", new[] { $"Bản điểm danh cho User ID '{targetUserId}' và Class Session ID '{targetClassSessionId}' đã tồn tại với ID khác." } }
                });
            }
        }

        if (updateAttendanceDto.Status.HasValue) 
        {
            existingAttendance.status_id = updateAttendanceDto.Status.Value;
        }
        if (updateAttendanceDto.Note != null)
        {
            existingAttendance.note = updateAttendanceDto.Note;
        }
        
        existingAttendance.check_at = DateTime.Now;

        try
        {
            await _unitOfWork.Attendances.UpdateAsync(existingAttendance);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật bản điểm danh trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the attendance.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var attendanceToDelete = await _unitOfWork.Attendances.GetByIdAsync(id);
        if (attendanceToDelete == null)
        {
            throw new NotFoundException("Attendance", "Id", id);
        }

        try
        {
            await _unitOfWork.Attendances.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi xóa bản điểm danh khỏi cơ sở dữ liệu. Có thể có các bản ghi liên quan.", dbEx, (int)HttpStatusCode.Conflict);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the attendance.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<AttendanceDto>> SearchAttendancesAsync(
        int? statusId = null,
        string? note = null,
        int? userId = null,
        int? classSessionId = null)
    {
        var attendances = await _unitOfWork.Attendances.SearchAttendancesWithDetailsAsync(
            statusId, note, userId, classSessionId
        );
        return attendances.Select(MapToAttendanceDto);
    }
    
    public async Task BulkUpdateAsync(BulkUpdateAttendanceDto bulkUpdateDto)
    {
        // Kiểm tra class session tồn tại
        var classSessionExists = await _unitOfWork.ClassSessions.GetByIdAsync(bulkUpdateDto.ClassSessionId);
        if (classSessionExists == null)
        {
            throw new NotFoundException("ClassSession", "Id", bulkUpdateDto.ClassSessionId);
        }

        var attendancesToUpdate = new List<attendance>();

        foreach (var attendanceItem in bulkUpdateDto.Attendances)
        {
            // Kiểm tra user tồn tại
            var userExists = await _unitOfWork.Users.GetByIdAsync(attendanceItem.UserId);
            if (userExists == null)
            {
                throw new NotFoundException("User", "Id", attendanceItem.UserId);
            }

            // Kiểm tra status tồn tại
            var statusExists = await _unitOfWork.AttendanceStatuses.GetByIdAsync(attendanceItem.Status);
            if (statusExists == null)
            {
                throw new NotFoundException("AttendanceStatus", "Id", attendanceItem.Status);
            }

            // Tìm attendance hiện tại hoặc tạo mới (dùng SearchAttendancesWithDetailsAsync để lấy entity)
            var attendances = await _unitOfWork.Attendances.SearchAttendancesWithDetailsAsync(
                userId: attendanceItem.UserId,
                classSessionId: bulkUpdateDto.ClassSessionId
            );
            var existingAttendance = attendances.FirstOrDefault();

            if (existingAttendance != null)
            {
                // Cập nhật attendance hiện tại
                existingAttendance.status_id = attendanceItem.Status;
                existingAttendance.note = attendanceItem.Note;
                existingAttendance.check_at = DateTime.Now;
                attendancesToUpdate.Add(existingAttendance);
            }
            else
            {
                // Tạo attendance mới
                var newAttendance = new attendance
                {
                    status_id = attendanceItem.Status,
                    check_at = DateTime.Now,
                    note = attendanceItem.Note,
                    user_id = attendanceItem.UserId,
                    class_session_id = bulkUpdateDto.ClassSessionId
                };
                attendancesToUpdate.Add(newAttendance);
            }
        }

        try
        {
            // Lưu tất cả thay đổi
            foreach (var attendance in attendancesToUpdate)
            {
                if (attendance.attendance_id == 0)
                {
                    // Thêm mới
                    await _unitOfWork.Attendances.AddAsync(attendance);
                }
                else
                {
                    // Cập nhật
                    await _unitOfWork.Attendances.UpdateAsync(attendance);
                }
            }
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật điểm danh hàng loạt.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while bulk updating attendance.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }
    
    private AttendanceDto MapToAttendanceDto(attendance att)
    {
        return new AttendanceDto
        {
            AttendanceId = att.attendance_id,
            StatusId = att.status_id,
            StatusName = att.status?.status_name,
            CheckAt = att.check_at,
            Note = att.note,
            UserId = att.user_id,
            ClassSessionId = att.class_session_id,
            ClassCode = att.class_session?._class?.class_code ?? "",
            
            User = att.user != null
                ? new UserDto
                {
                    UserId = att.user.user_id,
                    Username = att.user.username,
                    AccountName = att.user.account_name,
                    // ... các trường khác của User từ UserDto
                }
                : null,

            ClassSession = att.class_session != null
                ? new PersonalClassSessionDto
                {
                    ClassSessionId = att.class_session.class_session_id,
                    SessionNumber = att.class_session.session_number,
                    Date = att.class_session.date,
                    RoomCode = null, // Không include room
                    DayId = att.class_session.day_id,
                    ClassId = att.class_session.class_id,
                    TimeSlotId = att.class_session.time_slot_id,

                    DayOfWeekName = null, // Không include day
                    DateOfDay = null, // Không include day

                    WeekNumberInMonth = null, // Không include day.week

                    ClassCode = null, // Không include _class
                    InstrumentName = null, // Không include _class.instrument

                    StartTime = null, // Không include time_slot
                    EndTime = null // Không include time_slot
                }
                : null
        };
    }
}