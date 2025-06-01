using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class DocumentDto
{
    public int DocumentId { get; set; }
    public int? Lesson { get; set; }
    public string? LessonName { get; set; }
    public string Link { get; set; } = null!;
    public int InstrumentId { get; set; }

    // Có thể thêm DTO lồng nhau cho Instrument nếu cần trả về chi tiết
    // public InstrumentDto? Instrument { get; set; }
}

// DTO dùng làm input khi tạo mới Tài liệu (POST request body)
public class CreateDocumentDto
{
    public int? Lesson { get; set; }
    public string? LessonName { get; set; }

    [Required(ErrorMessage = "Link is required.")]
    [Url(ErrorMessage = "Invalid URL format for Link.")] // Validation cho URL (optional)
    [StringLength(500, ErrorMessage = "Link cannot exceed 500 characters.")]
    public string Link { get; set; } = null!;

    [Required(ErrorMessage = "Instrument ID is required.")]
    public int InstrumentId { get; set; }
}

// DTO dùng làm input khi cập nhật Tài liệu (PUT request body)
public class UpdateDocumentDto
{
    [Required(ErrorMessage = "Document ID is required for update.")]
    public int DocumentId { get; set; }

    public int? Lesson { get; set; }
    public string? LessonName { get; set; }

    [Url(ErrorMessage = "Invalid URL format for Link.")]
    [StringLength(500, ErrorMessage = "Link cannot exceed 500 characters.")]
    public string? Link { get; set; } // Có thể null khi update nếu client không muốn thay đổi link

    public int? InstrumentId { get; set; } // Có thể cập nhật Instrument ID
}