using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class StatisticDto
{
    public int StatisticId { get; set; }
    public DateOnly? Date { get; set; }
    public int? NewStudents { get; set; }
    public decimal? MonthlyRevenue { get; set; }
    public int? ConsultationCount { get; set; }
    public int? TotalStudents { get; set; }
    public int? ConsultationRequestCount { get; set; }
    public int? TotalGuitarClass { get; set; }
    public int? TotalPianoClass { get; set; }
}

// DTO dùng làm input khi tạo mới Thống kê (POST request body)
public class CreateStatisticDto
{
    // Nếu bạn muốn "date" luôn được tự động lấy ngày hiện tại và là NOT NULL trong DB
    // thì bạn có thể xóa thuộc tính này khỏi DTO và gán DateOnly.FromDateTime(DateTime.Now) trong service.
    // Tôi sẽ giả định nó có thể được cung cấp hoặc để trống (nullable) dựa trên model hiện tại.
    public DateOnly? Date { get; set; }

    public int? NewStudents { get; set; }
    public decimal? MonthlyRevenue { get; set; }
    public int? ConsultationCount { get; set; }
    public int? TotalStudents { get; set; }
    public int? ConsultationRequestCount { get; set; }
    public int? TotalGuitarClass { get; set; }
    public int? TotalPianoClass { get; set; }
}

// DTO dùng làm input khi cập nhật Thống kê (PUT request body)
public class UpdateStatisticDto
{
    [Required(ErrorMessage = "Statistic ID is required for update.")]
    public int StatisticId { get; set; }

    public DateOnly? Date { get; set; }
    public int? NewStudents { get; set; }
    public decimal? MonthlyRevenue { get; set; }
    public int? ConsultationCount { get; set; }
    public int? TotalStudents { get; set; }
    public int? ConsultationRequestCount { get; set; }
    public int? TotalGuitarClass { get; set; }
    public int? TotalPianoClass { get; set; }
}
