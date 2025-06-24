using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Services.Exceptions;

namespace Web_API.Middlewares;

public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger; // Đối tượng ghi log là non-static

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); 
            }
            catch (Exception ex)
            {
                // GHI LOG LỖI: Sử dụng _logger non-static ở đây
                _logger.LogError(ex, "An unhandled exception occurred during request processing: {Message}", ex.Message);
                
                // Truyền _logger vào phương thức HandleExceptionAsync vì nó là static
                await HandleExceptionAsync(context, ex, _logger); // <-- Đã thêm _logger vào tham số
            }
        }

        // Phương thức này bây giờ nhận ILogger<ExceptionHandlingMiddleware> làm tham số
        private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<ExceptionHandlingMiddleware> logger) // <-- Đã thêm tham số logger
        {
            context.Response.ContentType = "application/problem+json";
            
            var statusCode = HttpStatusCode.InternalServerError;
            var title = "An internal server error occurred.";
            var detail = "An unexpected error occurred during request processing.";
            Dictionary<string, string[]> errors = null;

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    statusCode = HttpStatusCode.NotFound;
                    title = "Resource Not Found";
                    detail = notFoundEx.Message;
                    break;

                case ValidationException validationEx:
                    statusCode = HttpStatusCode.BadRequest;
                    title = "Validation Error";
                    detail = validationEx.Message;
                    errors = (Dictionary<string, string[]>?)validationEx.Errors; // Lỗi này sẽ được sửa nếu ValidationException.Errors có kiểu đúng
                    break;

                case ForbiddenException forbiddenEx:
                    statusCode = HttpStatusCode.Forbidden;
                    title = "Forbidden";
                    detail = forbiddenEx.Message;
                    break;

                case UnauthorizedAppException unauthorizedEx:
                    statusCode = HttpStatusCode.Unauthorized;
                    title = "Unauthorized Access";
                    detail = unauthorizedEx.Message;
                    break;

                case ApiException apiEx:
                    statusCode = (HttpStatusCode)apiEx.StatusCode;
                    title = statusCode.ToString();
                    detail = apiEx.Message;
                    break;
                
                // Sửa lỗi Local variable 'argEx' might not be initialized
                // Bằng cách bắt ArgumentException chung, vì ArgumentNullException cũng kế thừa từ nó
                case ArgumentException commonArgEx: // <-- Đã sửa ở đây, sử dụng commonArgEx
                    statusCode = HttpStatusCode.BadRequest;
                    title = "Bad Request";
                    detail = commonArgEx.Message; // <-- Sử dụng commonArgEx
                    break;
                // Nếu bạn muốn xử lý riêng ArgumentNullException, hãy đặt nó trước ArgumentException
                // case ArgumentNullException argNullEx:
                //    statusCode = HttpStatusCode.BadRequest;
                //    title = "Null Argument Error";
                //    detail = argNullEx.Message;
                //    break;

                case InvalidOperationException invalidOpEx:
                    statusCode = HttpStatusCode.BadRequest;
                    title = "Invalid Operation";
                    detail = invalidOpEx.Message;
                    break;

                case Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateEx:
                    statusCode = HttpStatusCode.Conflict;
                    title = "Database Conflict";
                    detail = "A database update conflict occurred, possibly due to a unique constraint violation or related data issues.";
                    // Sử dụng logger được truyền vào
                    logger.LogError(dbUpdateEx, "DbUpdateException caught in middleware: {Message}", dbUpdateEx.Message); // <-- Đã sửa ở đây
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{(int)statusCode}",
                Title = title,
                Status = (int)statusCode,
                Detail = detail,
                Instance = context.Request.Path
            };

            if (errors != null && errors.Any())
            {
                problemDetails.Extensions["errors"] = errors;
            }

            var jsonResponse = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return context.Response.WriteAsync(jsonResponse);
        }
    }