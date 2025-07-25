using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class RoomDto
{
    public int RoomId { get; set; }
    public string RoomCode { get; set; }
    public int? Capacity { get; set; }
    public string? Description { get; set; }
}

public class CreateRoomDto
{
    [Required(ErrorMessage = "Mã phòng là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã phòng không được vượt quá 50 ký tự.")]
    public string RoomCode { get; set; } = null!;

    public int? Capacity { get; set; }

    [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
    public string? Description { get; set; }
}

public class UpdateRoomDto
{
    [Required(ErrorMessage = "ID phòng là bắt buộc.")]
    public int RoomId { get; set; }

    [StringLength(50, ErrorMessage = "Mã phòng không được vượt quá 50 ký tự.")]
    public string? RoomCode { get; set; }

    public int? Capacity { get; set; }

    [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
    public string? Description { get; set; }
}