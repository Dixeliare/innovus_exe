using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class BaseClassSessionDto
{
    public int ClassSessionId { get; set; }
    public int? SessionNumber { get; set; }
    public DateOnly? Date { get; set; }
    public string? RoomCode { get; set; } // Sẽ được ánh xạ từ model.room.room_code
    public int DayId { get; set; } // NON-NULLABLE: Phù hợp với model
    public int ClassId { get; set; } // NON-NULLABLE: Phù hợp với model
    public int TimeSlotId { get; set; } // NON-NULLABLE: Phù hợp với model
}

// DTO dùng làm input khi tạo mới Class Session (POST request body)
public class CreateClassSessionDto
{
    public int? SessionNumber { get; set; }
    public DateOnly? Date { get; set; }

    [Required(ErrorMessage = "ID phòng là bắt buộc.")] // Thêm Required
    public int RoomId { get; set; } // ĐÃ SỬA: Thay thế RoomCode bằng RoomId

    [Required(ErrorMessage = "ID ngày là bắt buộc.")] // Thêm Required
    public int DayId { get; set; }

    [Required(ErrorMessage = "ID lớp là bắt buộc.")] // Thêm Required
    public int ClassId { get; set; }

    [Required(ErrorMessage = "ID khung giờ là bắt buộc.")] // Thêm Required
    public int TimeSlotId { get; set; }
}

// DTO dùng làm input khi cập nhật Class Session (PUT request body)
public class UpdateClassSessionDto
{
    [Required(ErrorMessage = "Class Session ID là bắt buộc cho việc cập nhật.")]
    public int ClassSessionId { get; set; }

    public int? SessionNumber { get; set; }
    public DateOnly? Date { get; set; }
    public int? RoomId { get; set; } // ĐÃ SỬA: Thay thế RoomCode bằng RoomId (nullable)

    public int? DayId { get; set; }
    public int? ClassId { get; set; }
    public int? TimeSlotId { get; set; }
}

// DTO dùng để trả về thông tin chi tiết của Class Session (bao gồm các thông tin liên quan)
public class PersonalClassSessionDto
{
    public int ClassSessionId { get; set; }
    public int? SessionNumber { get; set; }
    public DateOnly? Date { get; set; }
    public string? RoomCode { get; set; } // Sẽ được ánh xạ từ model.room.room_code
    public int DayId { get; set; } // ĐÃ SỬA: Thay đổi từ int? sang int (phù hợp với model)
    public int ClassId { get; set; } // ĐÃ SỬA: Thay đổi từ int? sang int (phù hợp với model)
    public int TimeSlotId { get; set; } // ĐÃ SỬA: Thay đổi từ int? sang int (phù hợp với model)
    
    // Navigation properties' details
    public string? DayOfWeekName { get; set; }
    public DateOnly? DateOfDay { get; set; }
    public int? WeekId { get; set; } // Thêm WeekId nếu muốn hiển thị
    public int? WeekNumberInMonth { get; set; }
    public string? ClassCode { get; set; }
    public string? InstrumentName { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}