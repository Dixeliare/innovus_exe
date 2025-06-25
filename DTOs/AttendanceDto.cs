using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class AttendanceDto
{
    public int AttendanceId { get; set; }
    public bool? Status { get; set; }
    public DateTime? CheckAt { get; set; }
    public string? Note { get; set; }
    public int UserId { get; set; }
    public int ClassSessionId { get; set; }

    // Có thể thêm DTOs cho các navigation properties nếu cần hiển thị chi tiết
    // public UserDto? User { get; set; }
    // public ClassSessionDto? ClassSession { get; set; }
}

public class CreateAttendanceDto
{
    // Status có thể là optional, tùy thuộc vào logic nghiệp vụ của bạn
    // Ví dụ: khi tạo mới, status mặc định là true (có mặt)
    // Hoặc nếu nó là nullable trong DB, bạn không cần Required
    public bool? Status { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "User ID là bắt buộc.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Class Session ID là bắt buộc.")]
    public int ClassSessionId { get; set; }
}

public class UpdateAttendanceDto
{
    [Required(ErrorMessage = "Attendance ID là bắt buộc cho cập nhật.")]
    public int AttendanceId { get; set; }

    public bool? Status { get; set; }

    // CheckAt có thể là optional khi update.
    // Nếu bạn muốn nó luôn được cập nhật thành thời gian hiện tại khi bản ghi được sửa,
    // thì không cần trường này ở đây, hãy xử lý trong Service.
    public DateTime? CheckAt { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    public string? Note { get; set; }

    // Nếu bạn cho phép cập nhật User/ClassSession cho một bản ghi điểm danh
    // (thường thì không nên, vì mỗi bản ghi điểm danh là duy nhất cho cặp User-ClassSession)
    public int? UserId { get; set; }
    public int? ClassSessionId { get; set; }
}