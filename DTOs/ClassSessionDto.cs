using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class BaseClassSessionDto
{
    public int ClassSessionId { get; set; }
    public int? SessionNumber { get; set; }
    public DateOnly? Date { get; set; }
    public string RoomCode { get; set; } = null!;
    public int DayId { get; set; }
    public int ClassId { get; set; }
    public int TimeSlotId { get; set; }
}

// DTO dùng làm input khi tạo mới Class Session (POST request body)
public class CreateClassSessionDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Session Number must be a positive integer.")]
    public int? SessionNumber { get; set; } // Có thể null

    // DateOnly: Nếu cột `date` trong DB là nullable, thì dùng DateOnly?. Nếu là NOT NULL, dùng DateOnly.
    // Dựa trên model của bạn: 'DateOnly? date', vậy nó là nullable.
    public DateOnly? Date { get; set; } // Đã đổi thành nullable để khớp với model

    [Required(ErrorMessage = "Room Code is required.")]
    [StringLength(50, ErrorMessage = "Room Code cannot exceed 50 characters.")]
    public string RoomCode { get; set; } = null!;

    [Required(ErrorMessage = "Day ID is required.")] // Đã thay WeekId bằng DayId
    public int DayId { get; set; }

    [Required(ErrorMessage = "Class ID is required.")]
    public int ClassId { get; set; }

    [Required(ErrorMessage = "Time Slot ID is required.")]
    public int TimeSlotId { get; set; }
}

// DTO dùng làm input khi cập nhật Class Session (PUT request body)
public class UpdateClassSessionDto
{
    [Required(ErrorMessage = "Class Session ID is required for update.")]
    public int ClassSessionId { get; set; }

    public int? SessionNumber { get; set; }
    public DateOnly? Date { get; set; }
    public string? RoomCode { get; set; }

    public int? DayId { get; set; } // Đã thay WeekId bằng DayId
    public int? ClassId { get; set; }
    public int? TimeSlotId { get; set; }
}

// DTO dùng để trả về thông tin chi tiết của Class Session (bao gồm các thông tin liên quan)
public class PersonalClassSessionDto
{
    public int ClassSessionId { get; set; }
    public int? SessionNumber { get; set; } // Có thể là nullable
    public DateOnly? Date { get; set; }     // Date là nullable
    public string? RoomCode { get; set; }

    public int DayId { get; set; } // DayId là non-nullable trong ClassSession entity
    public int ClassId { get; set; }
    public int TimeSlotId { get; set; }

    // Các chi tiết từ Day và Week:
    public int? WeekId { get; set; }            // WeekId có thể nullable nếu day?.week_id null
    public DateOnly? DateOfDay { get; set; }    // Thêm thuộc tính này, có thể nullable
    public string? DayOfWeekName { get; set; }

    // Các chi tiết từ Week (nếu cần hiển thị trực tiếp từ session.day.week)
    public int? WeekNumberInMonth { get; set; } // Nullable nếu day?.week null

    // Các chi tiết từ Class
    public string? ClassCode { get; set; }
    public string? InstrumentName { get; set; }

    // Các chi tiết từ TimeSlot (TimeSpan cũng có thể nullable nếu time_slot null)
    public TimeSpan? StartTime { get; set; }    // Đổi kiểu sang TimeSpan?
    public TimeSpan? EndTime { get; set; }  
}