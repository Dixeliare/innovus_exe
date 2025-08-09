using System.Security.Claims;
using Services.Exceptions;

namespace Web_API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Lấy UserId từ ClaimsPrincipal
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAppException("Invalid user token");
            }
            return userId;
        }

        /// <summary>
        /// Lấy Username từ ClaimsPrincipal
        /// </summary>
        public static string GetUsername(this ClaimsPrincipal user)
        {
            var usernameClaim = user.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(usernameClaim))
            {
                throw new UnauthorizedAppException("Invalid user token");
            }
            return usernameClaim;
        }

        /// <summary>
        /// Lấy Role từ ClaimsPrincipal
        /// </summary>
        public static string GetUserRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleClaim))
            {
                throw new UnauthorizedAppException("Invalid user token");
            }
            return roleClaim;
        }

        /// <summary>
        /// Kiểm tra user có role cụ thể không
        /// </summary>
        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            var userRole = user.GetUserRole();
            return userRole == role;
        }

        /// <summary>
        /// Kiểm tra user có phải là admin không
        /// </summary>
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.HasRole("1");
        }

        /// <summary>
        /// Lấy UserId an toàn (không throw exception)
        /// </summary>
        public static int? GetUserIdOrDefault(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }
    }
} 