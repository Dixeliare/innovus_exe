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
                detail = notFoundEx.Message; // Thông điệp Not Found đã cụ thể
                break;

            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                title = "Validation Error";
                // Lấy thông điệp lỗi cụ thể từ Errors dictionary cho detail
                if (validationEx.Errors != null && validationEx.Errors.Any())
                {
                    // Lấy thông điệp của lỗi đầu tiên (ví dụ: lỗi ClassCode)
                    detail = validationEx.Errors.First().Value.FirstOrDefault() ?? validationEx.Message;
                }
                else
                {
                    detail = validationEx.Message; // Fallback nếu dictionary Errors rỗng
                }
                errors = (Dictionary<string, string[]>?)validationEx.Errors; // Gán errors dictionary vào đây
                break;

            case ForbiddenException forbiddenEx:
                statusCode = HttpStatusCode.Forbidden;
                title = "Forbidden Access"; // Rõ ràng hơn
                detail = forbiddenEx.Message; // Thông điệp Forbidden đã cụ thể
                break;

            case UnauthorizedAppException unauthorizedEx:
                statusCode = HttpStatusCode.Unauthorized;
                title = "Unauthorized Access";
                detail = unauthorizedEx.Message; // Thông điệp Unauthorized đã cụ thể
                break;

            case ApiException apiEx:
                statusCode = (HttpStatusCode)apiEx.StatusCode;
                title = $"API Error - {statusCode}"; // Rõ ràng hơn, kèm mã lỗi
                detail = apiEx.Message; // Thông điệp API Exception đã cụ thể
                break;

            case ArgumentException commonArgEx:
                statusCode = HttpStatusCode.BadRequest;
                title = "Bad Request - Invalid Argument"; // Cụ thể hơn
                detail = commonArgEx.Message; // Thông điệp ArgumentException đã cụ thể
                break;

            case InvalidOperationException invalidOpEx:
                statusCode = HttpStatusCode.BadRequest; // Hoặc Conflict nếu phù hợp hơn
                title = "Invalid Operation";
                detail = invalidOpEx.Message; // Thông điệp InvalidOperationException đã cụ thể
                break;

            case Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateEx:
                statusCode = HttpStatusCode.Conflict;
                title = "Database Conflict";
                // Đối với DbUpdateException, thường không nên tiết lộ thông điệp chi tiết ra frontend
                // vì nó có thể chứa thông tin nhạy cảm về cấu trúc DB.
                // Giữ lại thông điệp chung chung cho detail là tốt cho Production.
                detail = "A database update conflict occurred, possibly due to a unique constraint violation or related data issues.";
                logger.LogError(dbUpdateEx, "DbUpdateException caught in middleware: {Message}", dbUpdateEx.Message);
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