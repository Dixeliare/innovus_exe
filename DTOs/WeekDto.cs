using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class WeekDto
{
    public int WeekId { get; set; }
    public int? ScheduleId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int WeekNumberInMonth { get; set; } // Đảm bảo tên này
    public int? NumActiveDays { get; set; }
    public bool IsActive { get; set; }

    public ScheduleDto? Schedule { get; set; }
    public List<DayDto>? Days { get; set; }
}

// DTO dùng làm input khi tạo mới Tuần (POST request body)
public class CreateWeekDto
{
    [Required(ErrorMessage = "Week number is required.")]
    [Range(1, 5, ErrorMessage = "Week number must be between 1 and 5.")] // Ví dụ, một tháng có tối đa 5 tuần
    public int WeekNumberInMonth { get; set; } // Đổi tên từ WeekNumber và làm nó Required

    [Required(ErrorMessage = "Start date is required.")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    public DateOnly EndDate { get; set; }

    [Required(ErrorMessage = "Schedule ID is required.")]
    public int ScheduleId { get; set; }

    // XÓA: DayOfWeek không thuộc về Week
}

// DTO dùng làm input khi cập nhật Tuần (PUT request body)
public class UpdateWeekDto
{
    [Required(ErrorMessage = "Week ID is required for update.")]
    public int WeekId { get; set; }

    [Range(1, 5, ErrorMessage = "Week number must be between 1 and 5.")]
    public int? WeekNumberInMonth { get; set; } // Đổi tên từ WeekNumber

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? NumActiveDays { get; set; } // Cho phép cập nhật số ngày hoạt động

    // Có thể cho phép cập nhật ScheduleId, nhưng cần cẩn thận
    public int? ScheduleId { get; set; }

    // XÓA: DayOfWeek không thuộc về Week
}