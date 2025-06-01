using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class ConsultationRequestDto
    {
        public int ConsultationRequestId { get; set; }
        public string? Fullname { get; set; }
        public string? ContactNumber { get; set; }
        public string Email { get; set; } = null!; // Giữ nguyên null! vì model gốc là vậy
        public string Note { get; set; } = null!; // Giữ nguyên null! vì model gốc là vậy
        public bool? HasContact { get; set; }
        public int? StatisticId { get; set; }
        public int? ConsultationTopicId { get; set; }

        // Có thể thêm DTO lồng nhau cho ConsultationTopic và Statistic nếu cần trả về chi tiết
        // public ConsultationTopicDto? ConsultationTopic { get; set; }
        // public StatisticDto? Statistic { get; set; }
    }

    // DTO dùng làm input khi tạo mới Yêu cầu tư vấn (POST request body)
    public class CreateConsultationRequestDto
    {
        public string? Fullname { get; set; }

        [Phone(ErrorMessage = "Invalid contact number format.")] // Validation cho số điện thoại (optional)
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")] // Validation cho email
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Note is required.")]
        [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")] // Ví dụ validation
        public string Note { get; set; } = null!;

        // Mặc định là false hoặc null trong DB, tùy thuộc vào nghiệp vụ
        // Nếu bạn muốn nó mặc định là false khi tạo mới, không cần thêm vào DTO này
        // và gán false trong Service. Nếu client có thể quyết định, thì giữ lại.
        // Tôi sẽ giả định nó được xử lý trong service.
        // public bool? HasContact { get; set; }

        public int? StatisticId { get; set; }
        public int? ConsultationTopicId { get; set; }
    }

    // DTO dùng làm input khi cập nhật Yêu cầu tư vấn (PUT request body)
    public class UpdateConsultationRequestDto
    {
        [Required(ErrorMessage = "Consultation Request ID is required for update.")]
        public int ConsultationRequestId { get; set; }

        public string? Fullname { get; set; }

        [Phone(ErrorMessage = "Invalid contact number format.")]
        public string? ContactNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string? Email { get; set; } // Có thể không bắt buộc khi update nếu client chỉ muốn update một phần

        public string? Note { get; set; }

        public bool? HasContact { get; set; } // Có thể cập nhật trạng thái đã liên hệ

        public int? StatisticId { get; set; }
        public int? ConsultationTopicId { get; set; }
    }