using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class UserFavoriteSheetDto
{
    public int UserId { get; set; }
    public int SheetMusicId { get; set; }
    public bool? IsFavorite { get; set; }
    
    // Thông tin chi tiết của bài hát
    public SheetMusicDto? SheetMusic { get; set; }
    
    // Thông tin chi tiết của user
    public UserDto? User { get; set; }
}

// DTO để tạo mới favorite
public class CreateUserFavoriteSheetDto
{
    [Required(ErrorMessage = "User ID is required.")]
    public int UserId { get; set; }
    
    [Required(ErrorMessage = "Sheet Music ID is required.")]
    public int SheetMusicId { get; set; }
    
    public bool? IsFavorite { get; set; } = true; // Mặc định là true
}

// DTO để cập nhật favorite
public class UpdateUserFavoriteSheetDto
{
    [Required(ErrorMessage = "User ID is required.")]
    public int UserId { get; set; }
    
    [Required(ErrorMessage = "Sheet Music ID is required.")]
    public int SheetMusicId { get; set; }
    
    public bool? IsFavorite { get; set; }
}

// DTO để lấy danh sách bài hát yêu thích của user
public class UserFavoriteSheetListDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public ICollection<SheetMusicDto> FavoriteSheetMusics { get; set; } = new List<SheetMusicDto>();
}