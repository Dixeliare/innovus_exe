using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class UserFavoriteSheetDto
{
    public int UserId { get; set; }
    public int SheetMusicId { get; set; }
    public bool? IsFavorite { get; set; }
}

// DTO dùng làm input khi tạo mới hoặc thêm/cập nhật mối quan hệ yêu thích
// Vì đây là khóa chính composite, chúng ta sẽ dùng cùng DTO cho cả tạo và cập nhật logic
public class CreateUserFavoriteSheetDto
{
    [Required(ErrorMessage = "User ID is required.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Sheet Music ID is required.")]
    public int SheetMusicId { get; set; }

    public bool? IsFavorite { get; set; } = true; // Mặc định là true khi thêm vào yêu thích
}

// DTO dùng làm input khi cập nhật trạng thái yêu thích (PUT request body)
public class UpdateUserFavoriteSheetDto
{
    [Required(ErrorMessage = "User ID is required for update.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Sheet Music ID is required for update.")]
    public int SheetMusicId { get; set; }

    [Required(ErrorMessage = "IsFavorite status is required for update.")]
    public bool IsFavorite { get; set; } // Buộc phải cung cấp trạng thái khi cập nhật
}