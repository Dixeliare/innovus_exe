using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class ScheduleDto
{
    public int ScheduleId { get; set; }
    public DateOnly? MonthYear { get; set; }
    public string? Note { get; set; }

    // KHÔNG có UserId trực tiếp ở đây, vì model không có public int? user_id
    // Nếu muốn trả về thông tin User, bạn cần DTO lồng nhau (nested DTO)
    // public UserDto? User { get; set; }
}

// DTO dùng làm input khi tạo mới Lịch biểu (POST request body)
public class CreateScheduleDto
{
    public DateOnly? MonthYear { get; set; }

    [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
    public string? Note { get; set; }

    // KHÔNG có UserId ở đây, vì model không có public int? user_id
    // Để gán User cho Schedule khi tạo/cập nhật, cần một cách khác (ví dụ: thông qua User controller hoặc bảng join)
}

// DTO dùng làm input khi cập nhật Lịch biểu (PUT request body)
public class UpdateScheduleDto
{
    [Required(ErrorMessage = "Schedule ID is required for update.")]
    public int ScheduleId { get; set; }

    public DateOnly? MonthYear { get; set; }

    [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
    public string? Note { get; set; }

    // KHÔNG có UserId ở đây
}


public class PersonalScheduleDto
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? AccountName { get; set; }
    public List<PersonalClassSessionDto> ScheduledSessions { get; set; } = new List<PersonalClassSessionDto>();
}