using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class SheetDto
{
    public int SheetId { get; set; }
    public string SheetUrl { get; set; } = null!; // KHÔNG có ? vì NOT NULL
    public int SheetMusicId { get; set; } // Giả định khóa ngoại này tồn tại
    // Có thể thêm DTO lồng nhau cho SheetMusic nếu cần trả về chi tiết
    // public SheetMusicDto? SheetMusic { get; set; }
}

// DTO dùng làm input khi tạo mới Tờ nhạc (POST request body)
public class CreateSheetDto
{
    [Required(ErrorMessage = "Sheet URL is required.")]
    [Url(ErrorMessage = "Invalid URL format for Sheet URL.")] // Validation cho URL (optional)
    [StringLength(500, ErrorMessage = "Sheet URL cannot exceed 500 characters.")]
    public string SheetUrl { get; set; } = null!;

    [Required(ErrorMessage = "Sheet Music ID is required.")]
    public int SheetMusicId { get; set; } // Giả định khóa ngoại này tồn tại và là bắt buộc khi tạo
}

// DTO dùng làm input khi cập nhật Tờ nhạc (PUT request body)
public class UpdateSheetDto
{
    [Required(ErrorMessage = "Sheet ID is required for update.")]
    public int SheetId { get; set; }

    [Url(ErrorMessage = "Invalid URL format for Sheet URL.")]
    [StringLength(500, ErrorMessage = "Sheet URL cannot exceed 500 characters.")]
    public string? SheetUrl { get; set; } // Có thể null khi update nếu client không muốn thay đổi URL

    public int? SheetMusicId { get; set; } // Có thể cập nhật Sheet Music ID (nullable khi update)
}