using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AttendanceRepository _attendanceRepository;
    
    public AttendanceService(AttendanceRepository attendanceService) => _attendanceRepository = attendanceService;
    
    public async Task<IEnumerable<attendance>> GetAllAsync()
    {
        return await _attendanceRepository.GetAllAsync();
    }

    public async Task<attendance> GetByIdAsync(int id)
    {
        return await _attendanceRepository.GetByIdAsync(id);
    }

    public async Task<AttendanceDto> AddAsync(CreateAttendanceDto createAttendanceDto)
        {
            // Lấy thời gian hiện tại của máy chủ, với Kind là Local (thích hợp cho timestamp without time zone)
            // hoặc chuyển đổi rõ ràng sang Kind.Unspecified từ UTC+7
            // DateTime.Now tự động lấy giờ local của server
            // Nếu bạn muốn giờ Việt Nam (GMT+7) và server có thể ở múi giờ khác:
            // var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Hoặc "Asia/Ho_Chi_Minh" trên Linux/macOS
            // var nowInVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            // var checkAtValue = DateTime.SpecifyKind(nowInVietnam, DateTimeKind.Unspecified);

            // Cách đơn giản nhất:
            var checkAtValue = DateTime.Now;

            var attendanceEntity = new attendance
            {
                status = createAttendanceDto.Status,
                check_at = DateTime.Now, // Luôn lấy thời gian hiện tại khi tạo mới
                note = createAttendanceDto.Note,
                user_id = createAttendanceDto.UserId,
                class_session_id = createAttendanceDto.ClassSessionId
            };

            var addedAttendance = await _attendanceRepository.AddAsync(attendanceEntity);
            return MapToAttendanceDto(addedAttendance);
        }

        // Method UpdateAsync (PUT) - Cần xử lý cẩn thận nếu bạn muốn cập nhật CheckAt
        public async Task UpdateAsync(UpdateAttendanceDto updateAttendanceDto)
        {
            var existingAttendance = await _attendanceRepository.GetByIdAsync(updateAttendanceDto.AttendanceId);

            if (existingAttendance == null)
            {
                throw new KeyNotFoundException($"Attendance with ID {updateAttendanceDto.AttendanceId} not found.");
            }

            existingAttendance.status = updateAttendanceDto.Status;
            existingAttendance.note = updateAttendanceDto.Note;
            existingAttendance.user_id = updateAttendanceDto.UserId;
            existingAttendance.class_session_id = updateAttendanceDto.ClassSessionId;

            // Về phần `check_at` khi UPDATE:
            // 1. Nếu bạn không muốn cho phép cập nhật `check_at` từ DTO, thì không cần gán lại nó.
            //    existingAttendance.check_at sẽ giữ nguyên giá trị cũ từ DB.
            // 2. Nếu bạn muốn cho phép cập nhật `check_at` từ DTO, nhưng nếu DTO không cung cấp thì giữ nguyên:
            //    if (updateAttendanceDto.CheckAt.HasValue)
            //    {
            //        existingAttendance.check_at = DateTime.SpecifyKind(updateAttendanceDto.CheckAt.Value, DateTimeKind.Unspecified);
            //    }
            // 3. Nếu bạn muốn luôn cập nhật `check_at` thành thời gian hiện tại khi bản ghi được update:
            //    existingAttendance.check_at = DateTime.Now;
            //    Tùy vào nghiệp vụ của bạn, hãy chọn cách phù hợp.
            //    Trong ví dụ này, tôi giả định bạn sẽ không cập nhật check_at khi update,
            //    nếu không thì giữ nguyên giá trị cũ.
            //    Nếu bạn muốn cập nhật thời gian hiện tại khi update:
            existingAttendance.check_at = DateTime.Now; // Luôn cập nhật thời gian hiện tại khi sửa
            // Hoặc chỉ cập nhật nếu DTO cung cấp:
            // if (updateAttendanceDto.CheckAt.HasValue)
            // {
            //     existingAttendance.check_at = DateTime.SpecifyKind(updateAttendanceDto.CheckAt.Value, DateTimeKind.Unspecified);
            // }


            await _attendanceRepository.UpdateAsync(existingAttendance);
        }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _attendanceRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<attendance>> SearchAttendancesAsync(bool? status = null, string? note = null)
    {
        return await _attendanceRepository.SearchAttendancesAsync(status, note);
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