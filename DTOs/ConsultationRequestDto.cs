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

    // DTOs cho các thực thể liên quan (đã bỏ comment và được sử dụng để ánh xạ)
    public ConsultationTopicDto? ConsultationTopic { get; set; }
    public StatisticDto? Statistic { get; set; }

    public DateTime? HandledAt { get; set; } // <--- ĐÃ THÊM: Thời gian xử lý
    public UserForConsultationRequestDto? HandledBy { get; set; }
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

    [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")]
    public string? Note { get; set; }

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

// DTO cho thông tin người dùng xử lý
public class UserForConsultationRequestDto
{
    public int UserId { get; set; }
    public string? AccountName { get; set; } // Giả sử 'account_name' là tên hiển thị trong mô hình người dùng của bạn
    // Thêm các thuộc tính người dùng khác nếu cần hiển thị
}

public class UpdateConsultationRequestContactStatusDto
{
    [Required(ErrorMessage = "Consultation Request ID is required.")]
    public int ConsultationRequestId { get; set; }

    [Required(ErrorMessage = "HasContact status is required.")]
    public bool HasContact { get; set; } // Sử dụng bool không nullable vì client sẽ gửi giá trị true/false cụ thể
}