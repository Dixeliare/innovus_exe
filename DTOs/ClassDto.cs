namespace DTOs;

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
    public string? ClassCode { get; set; }
    public int InstrumentId { get; set; }
}

public class UpdateClassDto
{
    public int ClassId { get; set; }
    public string? ClassCode { get; set; }
    public int InstrumentId { get; set; }
}
