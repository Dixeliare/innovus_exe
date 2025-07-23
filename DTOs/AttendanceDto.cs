using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class AttendanceDto
{
    public int AttendanceId { get; set; }
    public int StatusId { get; set; } // ĐÃ THÊM: Để khớp với status_id (int)
    public string? StatusName { get; set; } // ĐÃ THÊM: Để hiển thị tên trạng thái từ navigation property
    public DateTime? CheckAt { get; set; }
    public string? Note { get; set; }
    public int UserId { get; set; }
    public int ClassSessionId { get; set; }

    // ĐÃ THÊM: Thuộc tính cho UserDto (từ navigation property)
    public UserDto? User { get; set; } 

    // ĐÃ THÊM: Thuộc tính cho PersonalClassSessionDto (từ navigation property)
    public PersonalClassSessionDto? ClassSession { get; set; } 
}

public class CreateAttendanceDto
{
    // ĐÃ SỬA: Kiểu dữ liệu từ bool sang int (tương ứng với status_id)
    [Required]
    public int Status { get; set; } 

    public string? Note { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ClassSessionId { get; set; }
}

public class UpdateAttendanceDto
{
    [Required]
    public int AttendanceId { get; set; }

    // ĐÃ SỬA: Kiểu dữ liệu từ bool? sang int? (tương ứng với status_id)
    public int? Status { get; set; } 

    public string? Note { get; set; }
    public int? UserId { get; set; }
    public int? ClassSessionId { get; set; }
}