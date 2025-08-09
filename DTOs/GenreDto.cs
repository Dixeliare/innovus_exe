using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class GenreDto
{
    public int GenreId { get; set; }
    public string? GenreName { get; set; }
    // Thêm danh sách bài hát thuộc thể loại này
    public ICollection<SheetMusicDto> SheetMusics { get; set; } = new List<SheetMusicDto>();
}

// DTO đơn giản cho Genre không có circular reference
public class GenreBasicDto
{
    public int GenreId { get; set; }
    public string? GenreName { get; set; }
}

// DTO dùng làm input khi tạo mới Thể loại (POST request body)
public class CreateGenreDto
{
    // Thường thì tên thể loại không nên null
    [Required(ErrorMessage = "Genre Name is required.")]
    [StringLength(100, ErrorMessage = "Genre Name cannot exceed 100 characters.")]
    public string GenreName { get; set; } = null!;
}

// DTO dùng làm input khi cập nhật Thể loại (PUT request body)
public class UpdateGenreDto
{
    [Required(ErrorMessage = "Genre ID is required for update.")]
    public int GenreId { get; set; }

    [StringLength(100, ErrorMessage = "Genre Name cannot exceed 100 characters.")]
    public string? GenreName { get; set; } // Có thể null khi update nếu client không muốn thay đổi tên
}