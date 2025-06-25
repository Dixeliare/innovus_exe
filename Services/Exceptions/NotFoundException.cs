using System.Text.Json;

namespace Services.Exceptions;

public class NotFoundException: Exception
{
    public string ResourceName { get; }
    public string FieldName { get; } // Có thể là null nếu không áp dụng
    public object FieldValue { get; } // Có thể là null nếu không áp dụng

    // Constructor 1: Cho trường hợp tìm kiếm bằng một khóa đơn lẻ
    public NotFoundException(string resourceName, string fieldName, object fieldValue)
        : base($"'{resourceName}' với {fieldName} '{fieldValue}' không tìm thấy.")
    {
        ResourceName = resourceName;
        FieldName = fieldName;
        FieldValue = fieldValue;
    }

    // Constructor 2: Cho trường hợp tìm kiếm bằng nhiều khóa hoặc đối tượng định danh
    // Đây là constructor mà bạn muốn gọi trong trường hợp này.
    public NotFoundException(string resourceName, object identifiers)
        : base($"'{resourceName}' với các định danh '{JsonSerializer.Serialize(identifiers)}' không tìm thấy.")
    {
        ResourceName = resourceName;
        FieldName = "Identifiers"; // Tên trường chung cho các định danh
        FieldValue = identifiers;
    }

    // Constructor 3: Chỉ với thông điệp chung (nếu cần)
    public NotFoundException(string message) : base(message)
    {
        ResourceName = "Unknown";
        FieldName = "N/A";
        FieldValue = "N/A";
    }

    // Constructor 4: Với thông điệp và một innerException (nếu cần wrap một exception khác)
    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
        ResourceName = "Unknown";
        FieldName = "N/A";
        FieldValue = "N/A";
    }
}