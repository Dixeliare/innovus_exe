using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class WeekDto
{
    public int WeekId { get; set; }
    public int? WeekNumber { get; set; }
    public DateOnly? DayOfWeek { get; set; } // Giữ tên theo model gốc
    public int ScheduleId { get; set; }

    // Có thể thêm DTO lồng nhau cho Schedule nếu cần chi tiết hơn
    // public ScheduleDto? Schedule { get; set; }
}

// DTO dùng làm input khi tạo mới Tuần (POST request body)
public class CreateWeekDto
{
    public int? WeekNumber { get; set; }

    public DateOnly? DayOfWeek { get; set; }

    [Required(ErrorMessage = "Schedule ID is required.")]
    public int ScheduleId { get; set; }
}

// DTO dùng làm input khi cập nhật Tuần (PUT request body)
public class UpdateWeekDto
{
    [Required(ErrorMessage = "Week ID is required for update.")]
    public int WeekId { get; set; }

    public int? WeekNumber { get; set; }

    public DateOnly? DayOfWeek { get; set; }

    // Có thể cho phép cập nhật ScheduleId, nhưng cần cẩn thận
    public int? ScheduleId { get; set; }
}