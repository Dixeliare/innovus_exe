namespace Services.Exceptions;

public class NotFoundException: Exception
{
    public string ResourceName { get; }
    public string FieldName { get; }
    public object FieldValue { get; }

    public NotFoundException(string resourceName, string fieldName, object fieldValue) : base(
        $"'{resourceName}' not found with '{fieldName}': '{fieldValue}'")
    {
        ResourceName = resourceName;
        FieldName = fieldName;
        FieldValue = fieldValue;
    }
    
    public NotFoundException(string message) : base(message)
    {
    }

    // Constructor cho phép truyền vào một ngoại lệ gốc (inner exception)
    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}