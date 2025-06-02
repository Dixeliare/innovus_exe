using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class ClassSessionDto
    {
        public int ClassSessionId { get; set; }
        public int? SessionNumber { get; set; }
        public DateOnly? Date { get; set; }
        public string RoomCode { get; set; } = null!;
        public int WeekId { get; set; }
        public int ClassId { get; set; }
        public int TimeSlotId { get; set; }

        // Bạn có thể thêm các DTO lồng nhau cho các đối tượng liên quan nếu bạn muốn trả về thông tin chi tiết
        // Ví dụ:
        // public ClassDto Class { get; set; }
        // public TimeSlotDto TimeSlot { get; set; }
        // public WeekDto Week { get; set; }
    }

    // DTO dùng làm input khi tạo mới Class Session (POST request body)
    public class CreateClassSessionDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Session Number must be a positive integer.")] // Ví dụ validation
        public int? SessionNumber { get; set; }

        // Mặc định, DateOnly là non-nullable trong PostgreSQL.
        // Nếu trong DB cột `date` của bạn là NOT NULL, thì ở đây cũng nên là DateOnly (không có '?').
        // Nếu nó là nullable trong DB, thì DateOnly?.
        // Tôi sẽ giả định nó là NOT NULL vì DateOnly thường là required.
        [Required(ErrorMessage = "Date is required.")]
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "Room Code is required.")]
        [StringLength(50, ErrorMessage = "Room Code cannot exceed 50 characters.")]
        public string RoomCode { get; set; } = null!;

        [Required(ErrorMessage = "Week ID is required.")]
        public int WeekId { get; set; }

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
        public DateOnly? Date { get; set; } // Có thể nullable khi update nếu không muốn bắt buộc cập nhật
        public string? RoomCode { get; set; } // Có thể nullable khi update

        public int? WeekId { get; set; }
        public int? ClassId { get; set; }
        public int? TimeSlotId { get; set; }
    }