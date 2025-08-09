using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Services.Exceptions;
using Web_API.Extensions;

namespace Web_API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Lấy UserId từ JWT token
        /// </summary>
        protected int GetCurrentUserId() => User.GetUserId();

        /// <summary>
        /// Lấy Username từ JWT token
        /// </summary>
        protected string GetCurrentUsername() => User.GetUsername();

        /// <summary>
        /// Lấy Role từ JWT token
        /// </summary>
        protected string GetCurrentUserRole() => User.GetUserRole();

        /// <summary>
        /// Kiểm tra user có role cụ thể không
        /// </summary>
        protected bool HasRole(string role) => User.HasRole(role);

        /// <summary>
        /// Kiểm tra user có phải là admin không
        /// </summary>
        protected bool IsAdmin() => User.IsAdmin();

        /// <summary>
        /// Lấy UserId an toàn (không throw exception)
        /// </summary>
        protected int? GetCurrentUserIdOrDefault() => User.GetUserIdOrDefault();
    }
}