using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class ConsultationTopicDto
{
    public int ConsultationTopicId { get; set; }
    public string? ConsultationTopicName { get; set; }
}

// DTO dùng làm input khi tạo mới Chủ đề tư vấn (POST request body)
public class CreateConsultationTopicDto
{
    // Thường thì tên topic không nên null
    [Required(ErrorMessage = "Consultation Topic Name is required.")]
    [StringLength(255, ErrorMessage = "Consultation Topic Name cannot exceed 255 characters.")]
    public string ConsultationTopicName { get; set; } = null!;
}

// DTO dùng làm input khi cập nhật Chủ đề tư vấn (PUT request body)
public class UpdateConsultationTopicDto
{
    [Required(ErrorMessage = "Consultation Topic ID is required for update.")]
    public int ConsultationTopicId { get; set; }

    [StringLength(255, ErrorMessage = "Consultation Topic Name cannot exceed 255 characters.")]
    public string? ConsultationTopicName { get; set; } // Có thể null khi update nếu client không muốn thay đổi tên
}