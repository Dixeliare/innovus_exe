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
    // private readonly IAttendanceRepository _attendanceRepository;
    //
    // public AttendanceService(IAttendanceRepository attendanceService) => _attendanceRepository = attendanceService;
    
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AttendanceDto>> GetAllAsync()
    {
        var attendances = await _unitOfWork.Attendances.GetAllAsync();
        return attendances.Select(MapToAttendanceDto);
    }

    public async Task<AttendanceDto> GetByIdAsync(int id)
    {
        var attendance = await _unitOfWork.Attendances.GetByIdAsync(id);
        if (attendance == null)
        {
            throw new NotFoundException("Attendance", "Id", id);
        }
        return MapToAttendanceDto(attendance);
    }

    public async Task<AttendanceDto> AddAsync(CreateAttendanceDto createAttendanceDto)
    {
        // Kiểm tra sự tồn tại của User
        var userExists = await _unitOfWork.Users.GetByIdAsync(createAttendanceDto.UserId);
        if (userExists == null)
        {
            throw new NotFoundException("User", "Id", createAttendanceDto.UserId);
        }

        // Kiểm tra sự tồn tại của ClassSession
        var classSessionExists = await _unitOfWork.ClassSessions.GetByIdAsync(createAttendanceDto.ClassSessionId);
        if (classSessionExists == null)
        {
            throw new NotFoundException("ClassSession", "Id", createAttendanceDto.ClassSessionId);
        }

        // Kiểm tra tính duy nhất: Một User chỉ có một bản điểm danh cho một ClassSession
        var existingAttendance = await _unitOfWork.Attendances.FindOneAsync(att =>
            att.user_id == createAttendanceDto.UserId &&
            att.class_session_id == createAttendanceDto.ClassSessionId);

        if (existingAttendance != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Conflict", new[] { $"Bản điểm danh cho User ID '{createAttendanceDto.UserId}' và Class Session ID '{createAttendanceDto.ClassSessionId}' đã tồn tại." } }
            });
        }

        var attendanceEntity = new attendance
        {
            status = createAttendanceDto.Status,
            check_at = DateTime.Now, // Luôn lấy thời gian hiện tại khi tạo mới
            note = createAttendanceDto.Note,
            user_id = createAttendanceDto.UserId,
            class_session_id = createAttendanceDto.ClassSessionId
        };

        try
        {
            var addedAttendance = await _unitOfWork.Attendances.AddAsync(attendanceEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
            return MapToAttendanceDto(addedAttendance);
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

        // Kiểm tra và cập nhật UserId nếu có giá trị mới và khác giá trị cũ
        if (updateAttendanceDto.UserId.HasValue && updateAttendanceDto.UserId.Value != existingAttendance.user_id)
        {
            var userExists = await _unitOfWork.Users.GetByIdAsync(updateAttendanceDto.UserId.Value);
            if (userExists == null)
            {
                throw new NotFoundException("User", "Id", updateAttendanceDto.UserId.Value);
            }
            existingAttendance.user_id = updateAttendanceDto.UserId.Value;
        }

        // Kiểm tra và cập nhật ClassSessionId nếu có giá trị mới và khác giá trị cũ
        if (updateAttendanceDto.ClassSessionId.HasValue && updateAttendanceDto.ClassSessionId.Value != existingAttendance.class_session_id)
        {
            var classSessionExists = await _unitOfWork.ClassSessions.GetByIdAsync(updateAttendanceDto.ClassSessionId.Value);
            if (classSessionExists == null)
            {
                throw new NotFoundException("ClassSession", "Id", updateAttendanceDto.ClassSessionId.Value);
            }
            existingAttendance.class_session_id = updateAttendanceDto.ClassSessionId.Value;
        }

        // Kiểm tra tính duy nhất nếu User ID hoặc Class Session ID bị thay đổi
        if ((updateAttendanceDto.UserId.HasValue && updateAttendanceDto.UserId.Value != existingAttendance.user_id) ||
            (updateAttendanceDto.ClassSessionId.HasValue && updateAttendanceDto.ClassSessionId.Value != existingAttendance.class_session_id))
        {
            int targetUserId = updateAttendanceDto.UserId ?? existingAttendance.user_id;
            int targetClassSessionId = updateAttendanceDto.ClassSessionId ?? existingAttendance.class_session_id;

            var conflictingAttendance = await _unitOfWork.Attendances.FindOneAsync(att =>
                att.user_id == targetUserId &&
                att.class_session_id == targetClassSessionId &&
                att.attendance_id != updateAttendanceDto.AttendanceId); // Không phải chính nó

            if (conflictingAttendance != null)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Conflict", new[] { $"Bản điểm danh cho User ID '{targetUserId}' và Class Session ID '{targetClassSessionId}' đã tồn tại với ID khác." } }
                });
            }
        }

        // Cập nhật các trường còn lại
        existingAttendance.status = updateAttendanceDto.Status ?? existingAttendance.status; // Giữ nguyên nếu null
        existingAttendance.note = updateAttendanceDto.Note ?? existingAttendance.note; // Giữ nguyên nếu null

        // Xử lý CheckAt khi UPDATE:
        // Cách 1: Luôn cập nhật CheckAt thành thời gian hiện tại khi có bất kỳ cập nhật nào
        existingAttendance.check_at = DateTime.Now;
        // Cách 2: Chỉ cập nhật CheckAt nếu DTO cung cấp một giá trị mới (tùy nghiệp vụ)
        // if (updateAttendanceDto.CheckAt.HasValue)
        // {
        //     existingAttendance.check_at = updateAttendanceDto.CheckAt.Value;
        // }
        // Cách 3: Không bao giờ cập nhật CheckAt sau khi tạo (giữ nguyên thời gian điểm danh ban đầu)
        // (không làm gì ở đây)


        try
        {
            await _unitOfWork.Attendances.UpdateAsync(existingAttendance);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
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
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi xóa bản điểm danh khỏi cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the attendance.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<AttendanceDto>> SearchAttendancesAsync(bool? status = null, string? note = null)
    {
        var attendances = await _unitOfWork.Attendances.SearchAttendancesAsync(status, note);
        return attendances.Select(MapToAttendanceDto);
    }
    
    private AttendanceDto MapToAttendanceDto(attendance att)
    {
        return new AttendanceDto
        {
            AttendanceId = att.attendance_id,
            Status = att.status,
            CheckAt = att.check_at,
            Note = att.note,
            UserId = att.user_id,
            ClassSessionId = att.class_session_id
        };
    }
}