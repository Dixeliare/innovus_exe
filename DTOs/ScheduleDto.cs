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

public class CreateScheduleDto
{
    // Make MonthYear required if a schedule must have a specific month/year
    [Required(ErrorMessage = "MonthYear is required.")]
    public DateOnly? MonthYear { get; set; } // GIỮ NGUYÊN DateOnly? để khớp với Model

    [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
    public string? Note { get; set; }
}

public class UpdateScheduleDto
{
    [Required(ErrorMessage = "Schedule ID is required for update.")]
    public int ScheduleId { get; set; }

    [Required(ErrorMessage = "MonthYear is required.")]
    public DateOnly? MonthYear { get; set; } // GIỮ NGUYÊN DateOnly? để khớp với Model

    [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
    public string? Note { get; set; }
}


public class PersonalScheduleDto
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? AccountName { get; set; }
    public List<PersonalClassSessionDto> ScheduledSessions { get; set; } = new List<PersonalClassSessionDto>();
}