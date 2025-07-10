namespace Services.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; } 

    // Constructor 1: Chỉ có lỗi dạng dictionary
    public ValidationException(IDictionary<string, string[]> errors)
        // Lấy thông điệp lỗi đầu tiên từ dictionary để làm thông điệp chính
        : base(errors != null && errors.Any() ? errors.First().Value.First() : "Một hoặc nhiều lỗi xác thực đã xảy ra.")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    // Constructor 2: Chỉ có thông điệp lỗi dạng string (không có lỗi cụ thể cho các trường)
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    // Constructor 3: Có thông điệp lỗi dạng string VÀ innerException (khi không có lỗi cụ thể cho các trường)
    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
    }

    // Constructor MỚI: Có lỗi dạng dictionary VÀ innerException
    public ValidationException(IDictionary<string, string[]> errors, Exception innerException)
        : base("Một hoặc nhiều lỗi xác thực đã xảy ra.", innerException) // Truyền innerException lên base
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}