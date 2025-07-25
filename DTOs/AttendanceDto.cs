using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class AttendanceDto
{
    public int AttendanceId { get; set; }
    public int StatusId { get; set; }
    public string? StatusName { get; set; } // Tên trạng thái điểm danh
    public DateTime? CheckAt { get; set; }
    public string? Note { get; set; }
    public int UserId { get; set; }
    public int ClassSessionId { get; set; }

    public UserDto? User { get; set; } // Chi tiết người dùng
    public PersonalClassSessionDto? ClassSession { get; set; } // Chi tiết buổi học
}

public class CreateAttendanceDto
{
    [Required(ErrorMessage = "Trạng thái điểm danh là bắt buộc.")]
    public int Status { get; set; } // ID trạng thái điểm danh (e.g., 0, 1, 2)

    public string? Note { get; set; }

    [Required(ErrorMessage = "ID người dùng là bắt buộc.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "ID buổi học là bắt buộc.")]
    public int ClassSessionId { get; set; }
}

public class UpdateAttendanceDto
{
    [Required(ErrorMessage = "ID điểm danh là bắt buộc.")]
    public int AttendanceId { get; set; }

    public int? Status { get; set; } // ID trạng thái điểm danh (Nullable)

    public string? Note { get; set; }

    public int? UserId { get; set; }

    public int? ClassSessionId { get; set; }
}