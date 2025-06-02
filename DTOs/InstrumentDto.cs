using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class InstrumentDto
{
    public int InstrumentId { get; set; }
    public string? InstrumentName { get; set; }
}

// DTO dùng làm input khi tạo mới Nhạc cụ (POST request body)
public class CreateInstrumentDto
{
    // Thường thì tên nhạc cụ không nên null
    [Required(ErrorMessage = "Instrument Name is required.")]
    [StringLength(100, ErrorMessage = "Instrument Name cannot exceed 100 characters.")]
    public string InstrumentName { get; set; } = null!;
}

// DTO dùng làm input khi cập nhật Nhạc cụ (PUT request body)
public class UpdateInstrumentDto
{
    [Required(ErrorMessage = "Instrument ID is required for update.")]
    public int InstrumentId { get; set; }

    [StringLength(100, ErrorMessage = "Instrument Name cannot exceed 100 characters.")]
    public string? InstrumentName { get; set; } // Có thể null khi update nếu client không muốn thay đổi tên
}