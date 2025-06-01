using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class SheetMusicDto
    {
        public int SheetMusicId { get; set; }
        public int? Number { get; set; }
        public string? MusicName { get; set; }
        public string Composer { get; set; } = null!;
        public string CoverUrl { get; set; } = null!;
        public int? SheetQuantity { get; set; }
        public int? FavoriteCount { get; set; }
        public int? SheetId { get; set; } // Khóa ngoại sheet_id

        // Có thể thêm DTO lồng nhau cho Sheet nếu muốn trả về chi tiết Sheet liên quan
        // public SheetDto? Sheet { get; set; }

        // Đối với Many-to-Many, có thể trả về danh sách IDs hoặc DTOs đơn giản
        // public ICollection<int> GenreIds { get; set; } = new List<int>();
        // public ICollection<GenreDto> Genres { get; set; } = new List<GenreDto>();
    }

    // DTO dùng làm input khi tạo mới Bản nhạc (POST request body)
    public class CreateSheetMusicDto
    {
        public int? Number { get; set; }

        [StringLength(200, ErrorMessage = "Music Name cannot exceed 200 characters.")]
        public string? MusicName { get; set; }

        [Required(ErrorMessage = "Composer is required.")]
        [StringLength(100, ErrorMessage = "Composer name cannot exceed 100 characters.")]
        public string Composer { get; set; } = null!;

        [Required(ErrorMessage = "Cover URL is required.")]
        [Url(ErrorMessage = "Invalid URL format for Cover URL.")]
        [StringLength(500, ErrorMessage = "Cover URL cannot exceed 500 characters.")]
        public string CoverUrl { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Sheet Quantity must be a positive number.")]
        public int? SheetQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Favorite Count cannot be negative.")]
        public int? FavoriteCount { get; set; } = 0; // Mặc định là 0 khi tạo mới

        // Khóa ngoại đến Sheet (nếu có, và có thể nullable)
        public int? SheetId { get; set; }

        // KHÔNG BAO GỒM CÁC LIST CHO MANY-TO-MANY TẠI ĐÂY ĐỂ ĐƠN GIẢN HÓA
        // public ICollection<int>? GenreIds { get; set; }
    }

    // DTO dùng làm input khi cập nhật Bản nhạc (PUT request body)
    public class UpdateSheetMusicDto
    {
        [Required(ErrorMessage = "Sheet Music ID is required for update.")]
        public int SheetMusicId { get; set; }

        public int? Number { get; set; }

        [StringLength(200, ErrorMessage = "Music Name cannot exceed 200 characters.")]
        public string? MusicName { get; set; }

        [StringLength(100, ErrorMessage = "Composer name cannot exceed 100 characters.")]
        public string? Composer { get; set; }

        [Url(ErrorMessage = "Invalid URL format for Cover URL.")]
        [StringLength(500, ErrorMessage = "Cover URL cannot exceed 500 characters.")]
        public string? CoverUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Sheet Quantity must be a positive number.")]
        public int? SheetQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Favorite Count cannot be negative.")]
        public int? FavoriteCount { get; set; }

        public int? SheetId { get; set; } // Có thể cập nhật khóa ngoại sheet_id
    }