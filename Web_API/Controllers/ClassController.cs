using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        
        public ClassController(IClassService classService) => _classService = classService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassDto>>> GetAllAsync() // Trả về DTOs
        {
            var classes = await _classService.GetAllAsync();
            return Ok(classes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClassDto>> GetById(int id) // Trả về DTO
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var cls = await _classService.GetByIdAsync(id);
            return Ok(cls); // Service đã trả về DTO
        }

        [HttpGet("search_by_instrument_id_or_class_code")] // Đổi tên đường dẫn cho rõ ràng hơn
        public async Task<ActionResult<IEnumerable<ClassDto>>> SearchAsync( // Trả về DTOs
            [FromQuery] int? instrumentId = null,
            [FromQuery] string? classCode = null)
        {
            var classes = await _classService.SearchClassesAsync(instrumentId, classCode);
            return Ok(classes); // Service đã trả về DTOs
        }

        [HttpPost]
        public async Task<ActionResult<ClassDto>> Add([FromBody] CreateClassDto createClassDto)
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            var newClass = await _classService.AddAsync(createClassDto);
            return CreatedAtAction(nameof(GetById), new { id = newClass.ClassId }, newClass);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClassDto updateClassDto)
        {
            if (id != updateClassDto.ClassId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ClassId", new string[] { "ID lớp học trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            await _classService.UpdateAsync(updateClassDto);
            return NoContent(); // 204 No Content cho cập nhật thành công
        }

        [HttpDelete("{id}")] // Xóa theo ID từ URL
        public async Task<IActionResult> DeleteAsync(int id) // Trả về IActionResult
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ApiException nếu có lỗi.
            await _classService.DeleteAsync(id);
            return NoContent();
        }
        
        // THÊM ENDPOINT MỚI NÀY
        [HttpGet("{id}/with-users")]
        //[Authorize(Roles = "1,2")] // Cho phép các role có quyền xem thông tin lớp học với danh sách người dùng
        public async Task<ActionResult<ClassDto>> GetClassWithUsers(int id)
        {
            var cls = await _classService.GetClassWithUsersByIdAsync(id);
            return Ok(cls);
        }
        
        
        // Endpoint để lấy danh sách tất cả học viên và giáo viên có sẵn
        [HttpGet("available-users")]
        //[Authorize(Roles = "1,2")] // Ví dụ: chỉ role 1 (Admin) và 2 (quản lý) có thể xem
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAvailableStudentsAndTeachers()
        {
            var users = await _classService.GetAvailableStudentsAndTeachersAsync();
            return Ok(users);
        }

        // Endpoint để gán (thay thế) danh sách học viên/giáo viên cho một lớp
        // Điều này sẽ XÓA TẤT CẢ người dùng hiện có trong lớp và gán danh sách mới.
        [HttpPut("{classId}/users")]
        //[Authorize(Roles = "1")] // Ví dụ: chỉ role 1 (Admin) có thể gán người dùng
        public async Task<IActionResult> AssignUsersToClass(int classId, [FromBody] ManageClassUsersDto dto)
        {
            if (classId != dto.ClassId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ClassId", new string[] { "ID lớp học trong URL không khớp với ID trong body." } }
                });
            }
            await _classService.AssignUsersToClassAsync(classId, dto.UserIds);
            return NoContent(); // 204 No Content
        }

        // Endpoint để thêm người dùng vào một lớp hiện có
        // Điều này sẽ THÊM người dùng vào danh sách hiện có, không xóa.
        [HttpPost("{classId}/users/add")]
        //[Authorize(Roles = "1")]
        public async Task<IActionResult> AddUsersToClass(int classId, [FromBody] ManageClassUsersDto dto)
        {
            if (classId != dto.ClassId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ClassId", new string[] { "ID lớp học trong URL không khớp với ID trong body." } }
                });
            }
            await _classService.AddUsersToClassAsync(classId, dto.UserIds);
            return NoContent();
        }

        // Endpoint để xóa người dùng khỏi một lớp hiện có
        [HttpDelete("{classId}/users/remove")]
        //[Authorize(Roles = "1")]
        public async Task<IActionResult> RemoveUsersFromClass(int classId, [FromBody] ManageClassUsersDto dto)
        {
            if (classId != dto.ClassId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ClassId", new string[] { "ID lớp học trong URL không khớp với ID trong body." } }
                });
            }
            await _classService.RemoveUsersFromClassAsync(classId, dto.UserIds);
            return NoContent();
        }

    }
}
