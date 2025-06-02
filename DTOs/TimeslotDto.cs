using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class TimeslotDto
{
    public int TimeslotId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

// DTO dùng làm input khi tạo mới Khung giờ (POST request body)
public class CreateTimeslotDto
{
    [Required(ErrorMessage = "Start Time is required.")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "End Time is required.")]
    // Thêm validation để đảm bảo EndTime sau StartTime
    public TimeOnly EndTime { get; set; }
}

// DTO dùng làm input khi cập nhật Khung giờ (PUT request body)
public class UpdateTimeslotDto
{
    [Required(ErrorMessage = "Timeslot ID is required for update.")]
    public int TimeslotId { get; set; }

    public TimeOnly? StartTime { get; set; } // Có thể null khi update nếu không muốn thay đổi

    public TimeOnly? EndTime { get; set; } // Có thể null khi update nếu không muốn thay đổi
}