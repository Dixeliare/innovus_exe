using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class OpeningScheduleDto
    {
        public int OpeningScheduleId { get; set; }
        public string? Subject { get; set; }
        public string? ClassCode { get; set; }
        public DateOnly? OpeningDay { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Schedule { get; set; }
        public int? StudentQuantity { get; set; }
        public bool? IsAdvancedClass { get; set; }
    }

    // DTO dùng làm input khi tạo mới Lịch khai giảng (POST request body)
    public class CreateOpeningScheduleDto
    {
        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters.")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Class Code is required.")]
        [StringLength(50, ErrorMessage = "Class Code cannot exceed 50 characters.")]
        public string ClassCode { get; set; } = null!;

        [Required(ErrorMessage = "Opening Day is required.")]
        public DateOnly OpeningDay { get; set; }

        public DateOnly? EndDate { get; set; } // Có thể là null

        [Required(ErrorMessage = "Schedule is required.")]
        [StringLength(200, ErrorMessage = "Schedule cannot exceed 200 characters.")]
        public string Schedule { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Student Quantity must be a positive number.")]
        public int? StudentQuantity { get; set; }

        public bool? IsAdvancedClass { get; set; }
    }

    // DTO dùng làm input khi cập nhật Lịch khai giảng (PUT request body)
    public class UpdateOpeningScheduleDto
    {
        [Required(ErrorMessage = "Opening Schedule ID is required for update.")]
        public int OpeningScheduleId { get; set; }

        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters.")]
        public string? Subject { get; set; }

        [StringLength(50, ErrorMessage = "Class Code cannot exceed 50 characters.")]
        public string? ClassCode { get; set; }

        public DateOnly? OpeningDay { get; set; }

        public DateOnly? EndDate { get; set; }

        [StringLength(200, ErrorMessage = "Schedule cannot exceed 200 characters.")]
        public string? Schedule { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Student Quantity must be a positive number.")]
        public int? StudentQuantity { get; set; }

        public bool? IsAdvancedClass { get; set; }
    }