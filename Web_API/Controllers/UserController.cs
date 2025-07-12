using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Data;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public UserController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        [HttpPost("Login")]
        // Không cần thay đổi ở đây, vì login request chỉ cần username và password
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Controller gọi service. Nếu service ném UnauthorizedAppException,
            // Middleware sẽ bắt và trả về 401 Unauthorized.
            var user = await _userService.GetUserAccount(request.UserName, request.Password);

            // Đoạn code này chỉ chạy nếu user được trả về thành công (không ném Exception)
            var token = GenerateJSONWebToken(user);
            return Ok(new { token });
        }

        private string GenerateJSONWebToken(user systemUserAccount)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, systemUserAccount.username ?? systemUserAccount.user_id.ToString()), // Sử dụng username hoặc ID
                new(ClaimTypes.NameIdentifier, systemUserAccount.user_id.ToString()) // ID của người dùng
            };
            if (systemUserAccount.role != null) // Đảm bảo role đã được load
            {
                claims.Add(new(ClaimTypes.Role, systemUserAccount.role.role_id.ToString())); // Lấy role_id từ navigation property
            }

            var token = new JwtSecurityToken(_config["Jwt:Issuer"]
                , _config["Jwt:Audience"]
                , claims
                , expires: DateTime.UtcNow.AddMinutes(120)    //Now => UtcNow
                , signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }

        public sealed record LoginRequest(string UserName, string Password);

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), (int)HttpStatusCode.OK)]
        //[Authorize(Roles = "1,2")] // Cho phép cả role 1 và 2 xem danh sách users
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllAsync()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users); // Service đã trả về UserDto
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[Authorize(Roles = "1,2")] // Cho phép cả role 1 và 2 xem user theo ID
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }

        // GET: api/Users/username/{username}
        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[Authorize(Roles = "1,2")] // Cho phép cả role 1 và 2 xem user theo username
        public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
        {
            var user = await _userService.GetByUsernameAsync(username);
            return Ok(user);
        }
        
        // API MỚI: Lấy profile của người dùng hiện tại sau khi đăng nhập
        [HttpGet("profile")]
        [Authorize] // Yêu cầu xác thực để truy cập endpoint này
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)] // Nếu token không hợp lệ hoặc thiếu User ID
        [ProducesResponseType((int)HttpStatusCode.NotFound)] // Nếu User ID trong token không tìm thấy trong DB
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)] // Xử lý lỗi server
        public async Task<ActionResult<UserDto>> GetUserProfile()
        {
            // Lấy User ID từ Claims trong JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                // Điều này không nên xảy ra nếu JWT được tạo đúng cách và chứa User ID,
                // nhưng là một biện pháp phòng ngừa để xử lý các token không hợp lệ.
                return Unauthorized(new { message = "Không tìm thấy User ID trong token hoặc định dạng không hợp lệ." });
            }

            try
            {
                var userProfile = await _userService.GetByIdAsync(userId);
                if (userProfile == null)
                {
                    // Trường hợp này có thể xảy ra nếu người dùng đã bị xóa khỏi DB
                    // nhưng token của họ vẫn còn hiệu lực.
                    return NotFound(new { message = $"Không tìm thấy hồ sơ người dùng với ID '{userId}'." });
                }
                return Ok(userProfile);
            }
            catch (NotFoundException ex)
            {
                // Bắt NotFoundException từ service nếu có, trả về 404
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi không mong muốn khác xảy ra trong quá trình truy xuất dữ liệu
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "Đã xảy ra lỗi khi truy xuất hồ sơ người dùng.", details = ex.Message });
            }
        }


        // POST: api/Users
        [HttpPost]
        //[Authorize(Roles = "1")] // Chỉ role 1 được tạo user mới
        [Consumes("multipart/form-data")] // Quan trọng để nhận file
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        // Quan trọng: Thêm Consumes cho Form-data
        public async Task<ActionResult<UserDto>> CreateUser([FromForm] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về 400 Bad Request
            }

            // Middleware sẽ bắt ValidationException, NotFoundException, ApiException từ Service
            var createdUser = await _userService.AddAsync(
                createUserDto.Username,
                createUserDto.AccountName,
                createUserDto.Password, // Mật khẩu THÔ
                createUserDto.Address,
                createUserDto.PhoneNumber,
                createUserDto.IsDisabled,
                createUserDto.AvatarImageFile,
                createUserDto.Birthday,
                createUserDto.RoleId,
                createUserDto.StatisticId,
                createUserDto.OpeningScheduleId,
                createUserDto.ScheduleId,
                createUserDto.Email,
                createUserDto.GenderId,
                createUserDto.ClassId 
            );
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.UserId }, createdUser);
        }

        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "1,2")]
        [Consumes("multipart/form-data")] // Quan trọng để nhận file
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]// Quan trọng: Thêm Consumes cho Form-data
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDto updateUserDto)
        {
            if (id != updateUserDto.UserId)
            {
                // Ném ValidationException thay vì BadRequest nếu ID không khớp
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "UserId", new string[] { "ID người dùng trong URL không khớp với ID trong body." } }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Middleware sẽ bắt NotFoundException, ValidationException, ApiException từ Service
            await _userService.UpdateAsync(
                updateUserDto.UserId,
                updateUserDto.Username,
                updateUserDto.AccountName,
                updateUserDto.NewPassword, // Mật khẩu THÔ mới
                updateUserDto.Address,
                updateUserDto.PhoneNumber,
                updateUserDto.IsDisabled,
                updateUserDto.AvatarImageFile,
                updateUserDto.Birthday,
                updateUserDto.RoleId,
                updateUserDto.StatisticId,
                updateUserDto.OpeningScheduleId,
                updateUserDto.ScheduleId,
                updateUserDto.Email,
                updateUserDto.GenderId
            );
            return NoContent();
        }

        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "1")] // Chỉ role 1 được xóa user
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userService.DeleteAsync(id);
            return NoContent();

        }

        // GET: api/Users/search
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), (int)HttpStatusCode.OK)]
        [Authorize(Roles = "1,2")] // Cho phép cả role 1 và 2 tìm kiếm user
        public async Task<ActionResult<IEnumerable<UserDto>>> SearchUsers(
            [FromQuery] string? username,
            [FromQuery] string? accountName,
            [FromQuery] string? password, // Parameter này vẫn có ở đây, nhưng service sẽ bỏ qua nó
            [FromQuery] string? address,
            [FromQuery] string? phoneNumber,
            [FromQuery] bool? isDisabled,
            [FromQuery] DateTime? createAt,
            [FromQuery] DateOnly? birthday,
            [FromQuery] int? roleId,
            [FromQuery] string? email, 
            [FromQuery] int? genderId)
        {
            var users = await _userService.SearchUsersAsync(username, accountName, password, address, phoneNumber, isDisabled, createAt, birthday, roleId, email, genderId);
            return Ok(users); // Service đã trả về UserDto
        }

    }
}
