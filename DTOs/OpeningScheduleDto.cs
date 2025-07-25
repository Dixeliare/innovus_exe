using DTOs;

public class OpeningScheduleDto
{
    public int OpeningScheduleId { get; set; }
    public string ClassCode { get; set; } = null!;
    public DateOnly? OpeningDay { get; set; }
    public DateOnly? EndDate { get; set; }
    // ĐÃ XÓA: public string? Schedule { get; set; }
    public int? StudentQuantity { get; set; }
    public bool? IsAdvancedClass { get; set; }
    public UserForOpeningScheduleDto? TeacherUser { get; set; }
    public int InstrumentId { get; set; }
    public InstrumentDto? Instrument { get; set; }
    public int TotalSessions { get; set; }
    public List<int>? SelectedDayOfWeekIds { get; set; }
}

// DTO dùng làm input khi tạo mới Lịch khai giảng (POST request body)
public class CreateOpeningScheduleDto
{
    public string ClassCode { get; set; } = null!;
    public DateOnly OpeningDay { get; set; }
    public DateOnly? EndDate { get; set; }
    // ĐÃ XÓA: public string? Schedule { get; set; }
    public int? StudentQuantity { get; set; }
    public bool? IsAdvancedClass { get; set; }
    public int? TeacherUserId { get; set; }
    public int InstrumentId { get; set; }
    public int TotalSessions { get; set; }

    public List<int>? SelectedDayOfWeekIds { get; set; }
    public int DefaultRoomId { get; set; }
    public List<int> TimeSlotIds { get; set; } = new List<int>();
}

// DTO dùng làm input khi cập nhật Lịch khai giảng (PUT request body)
public class UpdateOpeningScheduleDto : CreateOpeningScheduleDto
{
    public int OpeningScheduleId { get; set; }
}

public class UserForOpeningScheduleDto
{
    public string? AccountName { get; set; }
}