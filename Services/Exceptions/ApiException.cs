using System.Net;

namespace Services.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode = (int)HttpStatusCode.InternalServerError) : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(string message, Exception innerException, int statusCode = (int)HttpStatusCode.InternalServerError) : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}