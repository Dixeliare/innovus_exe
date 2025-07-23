using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class DayDto
{
    public int DayId { get; set; }
    public int? WeekId { get; set; }
    public DateOnly DateOfDay { get; set; }
    public string DayOfWeekName { get; set; } = null!;
    public bool? IsActive { get; set; }

    // DTO của các Navigation Properties (nếu muốn trả về thông tin liên quan)
    public WeekDto? Week { get; set; } // Yêu cầu WeekDto
    public List<BaseClassSessionDto>? ClassSessions { get; set; } // Yêu cầu ClassSessionDto
}

public class CreateDayDto
{
    public int? WeekId { get; set; }

    [Required(ErrorMessage = "DateOfDay là bắt buộc.")]
    public DateOnly DateOfDay { get; set; }

    [Required(ErrorMessage = "DayOfWeekName là bắt buộc.")]
    [StringLength(10, ErrorMessage = "DayOfWeekName không được vượt quá 10 ký tự.")]
    public string DayOfWeekName { get; set; } = null!;

    public bool? IsActive { get; set; } = true; // Giá trị mặc định khi tạo nếu không được cung cấp
}

public class UpdateDayDto
{
    [Required(ErrorMessage = "DayId là bắt buộc khi cập nhật.")]
    public int DayId { get; set; }

    public int? WeekId { get; set; }

    [Required(ErrorMessage = "DateOfDay là bắt buộc.")]
    public DateOnly DateOfDay { get; set; }

    [Required(ErrorMessage = "DayOfWeekName là bắt buộc.")]
    [StringLength(10, ErrorMessage = "DayOfWeekName không được vượt quá 10 ký tự.")]
    public string DayOfWeekName { get; set; } = null!;

    public bool? IsActive { get; set; }
}