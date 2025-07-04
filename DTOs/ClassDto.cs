using System.ComponentModel.DataAnnotations;

public class ClassDto
{
    public int ClassId { get; set; }
    public string? ClassCode { get; set; }
    public int InstrumentId { get; set; }

    // Navigation properties
    // Bạn có thể thêm các DTO cho class_session và user nếu muốn trả về thông tin chi tiết
    // Ví dụ: public ICollection<ClassSessionDto> ClassSessions { get; set; } = new List<ClassSessionDto>();
    // public ICollection<UserDto> Users { get; set; } = new List<UserDto>();
}

public class CreateClassDto
{
    [Required(ErrorMessage = "Mã lớp học là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã lớp học không được vượt quá 50 ký tự.")]
    public string ClassCode { get; set; } = null!; // Đảm bảo không null khi tạo

    [Required(ErrorMessage = "ID nhạc cụ là bắt buộc.")]
    public int InstrumentId { get; set; }
}

public class UpdateClassDto
{
    [Required(ErrorMessage = "Class ID là bắt buộc cho cập nhật.")]
    public int ClassId { get; set; }

    [StringLength(50, ErrorMessage = "Mã lớp học không được vượt quá 50 ký tự.")]
    public string? ClassCode { get; set; } // Có thể null khi update nếu không muốn thay đổi mã lớp

    public int? InstrumentId { get; set; } // Có thể null khi update nếu không muốn thay đổi nhạc cụ
}