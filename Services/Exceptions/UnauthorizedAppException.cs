namespace Services.Exceptions;

public class UnauthorizedAppException: Exception
{
    public UnauthorizedAppException(string message) : base(message)
    {
    }

    public UnauthorizedAppException(string message, Exception innerException) : base(message, innerException)
    {
    }
}