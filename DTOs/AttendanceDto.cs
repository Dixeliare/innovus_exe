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
    public bool? Status { get; set; }// Có thể không cần nếu DB tự đặt default CURRENT_TIMESTAMP
    public string? Note { get; set; }
    public int UserId { get; set; }
    public int ClassSessionId { get; set; }
}

public class UpdateAttendanceDto
{
    public int AttendanceId { get; set; }
    public bool? Status { get; set; }
    public DateTime? CheckAt { get; set; }
    public string? Note { get; set; }
    public int UserId { get; set; }
    public int ClassSessionId { get; set; }
}