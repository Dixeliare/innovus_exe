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
using System.Net; // Thêm namespace này cho HttpStatusCode
using Microsoft.AspNetCore.Authorization; // Thêm cho Authorize

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        
        public ClassController(IClassService classService) => _classService = classService;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClassDto>), (int)HttpStatusCode.OK)]
        // [Authorize(Roles = "1,2,3")] // Example: Admin, Manager, Teacher can view classes
        public async Task<ActionResult<IEnumerable<ClassDto>>> GetAllAsync()
        {
            var classes = await _classService.GetAllAsync();
            return Ok(classes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClassDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        // [Authorize(Roles = "1,2,3")] // Example: Admin, Manager, Teacher can view a class by ID
        public async Task<ActionResult<ClassDto>> GetById(int id)
        {
            var cls = await _classService.GetByIdAsync(id);
            return Ok(cls);
        }

        [HttpGet("search_by_instrument_id_or_class_code")]
        [ProducesResponseType(typeof(IEnumerable<ClassDto>), (int)HttpStatusCode.OK)]
        // [Authorize(Roles = "1,2,3")] // Example: Admin, Manager, Teacher can search classes
        public async Task<ActionResult<IEnumerable<ClassDto>>> SearchAsync(
            [FromQuery] int? instrumentId = null,
            [FromQuery] string? classCode = null)
        {
            var classes = await _classService.SearchClassesAsync(instrumentId, classCode);
            return Ok(classes);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ClassDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)] // For validation errors
        [ProducesResponseType((int)HttpStatusCode.NotFound)] // For invalid InstrumentId
        [ProducesResponseType((int)HttpStatusCode.Conflict)] // For duplicate ClassCode
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        // [Authorize(Roles = "1,2")] // Example: Only Admin, Manager can create classes
        public async Task<ActionResult<ClassDto>> Add([FromBody] CreateClassDto createClassDto)
        {
            var newClass = await _classService.AddAsync(createClassDto);
            return CreatedAtAction(nameof(GetById), new { id = newClass.ClassId }, newClass);
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)] // For ID mismatch or validation errors
        [ProducesResponseType((int)HttpStatusCode.Conflict)] // For duplicate ClassCode after update
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        // [Authorize(Roles = "1,2")] // Example: Only Admin, Manager can update classes
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClassDto updateClassDto)
        {
            if (id != updateClassDto.ClassId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ClassId", new string[] { "ID lớp học trong URL không khớp với ID trong body." } }
                });
            }

            await _classService.UpdateAsync(updateClassDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)] // If related sessions or users exist
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        // [Authorize(Roles = "1,2")] // Example: Only Admin, Manager can delete classes
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _classService.DeleteAsync(id);
            return NoContent();
        }
        
        [HttpGet("{id}/with-users")]
        [ProducesResponseType(typeof(ClassDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        // [Authorize(Roles = "1,2")] // Allow roles with permission to view class info with user list
        public async Task<ActionResult<ClassDto>> GetClassWithUsers(int id)
        {
            var cls = await _classService.GetClassWithUsersByIdAsync(id);
            return Ok(cls);
        }
        
        [HttpGet("available-users")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)] // If Student or Teacher roles are not found
        // [Authorize(Roles = "1,2")] // Example: only role 1 (Admin) and 2 (Manager) can view
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAvailableStudentsAndTeachers()
        {
            var users = await _classService.GetAvailableStudentsAndTeachersAsync();
            return Ok(users);
        }

        [HttpPut("{classId}/users")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)] // For ID mismatch or validation errors
        [ProducesResponseType((int)HttpStatusCode.NotFound)] // For invalid ClassId or UserIds
        [ProducesResponseType((int)HttpStatusCode.PreconditionFailed)] // If required roles are not found
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        // [Authorize(Roles = "1")] // Example: only role 1 (Admin) can assign users
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
            return NoContent();
        }

        [HttpPost("{classId}/users/add")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)] // For ID mismatch or validation errors
        [ProducesResponseType((int)HttpStatusCode.NotFound)] // For invalid ClassId or UserIds
        [ProducesResponseType((int)HttpStatusCode.PreconditionFailed)] // If required roles are not found
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        // [Authorize(Roles = "1")]
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

        [HttpDelete("{classId}/users/remove")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)] // For ID mismatch or validation errors
        [ProducesResponseType((int)HttpStatusCode.NotFound)] // For invalid ClassId
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        // [Authorize(Roles = "1")]
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

        [HttpGet("{classId}/student-capacity")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetStudentCapacityInfo(int classId)
        {
            var cls = await _classService.GetByIdAsync(classId);
            
            var capacityInfo = new
            {
                ClassId = cls.ClassId,
                ClassCode = cls.ClassCode,
                TotalStudents = cls.TotalStudents,
                CurrentStudentsCount = cls.CurrentStudentsCount,
                AvailableSlots = cls.TotalStudents > 0 ? cls.TotalStudents - cls.CurrentStudentsCount : int.MaxValue,
                IsAtCapacity = cls.TotalStudents > 0 && cls.CurrentStudentsCount >= cls.TotalStudents,
                CanAddStudents = cls.TotalStudents == 0 || cls.CurrentStudentsCount < cls.TotalStudents
            };

            return Ok(capacityInfo);
        }

        [HttpPost("{classId}/check-can-add-students")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CheckCanAddStudents(int classId, [FromBody] List<int> studentIds)
        {
            var cls = await _classService.GetByIdAsync(classId);
            
            var result = new
            {
                ClassId = cls.ClassId,
                ClassCode = cls.ClassCode,
                TotalStudents = cls.TotalStudents,
                CurrentStudentsCount = cls.CurrentStudentsCount,
                StudentsToAdd = studentIds.Count,
                CanAdd = cls.TotalStudents == 0 || (cls.CurrentStudentsCount + studentIds.Count) <= cls.TotalStudents,
                Message = cls.TotalStudents == 0 
                    ? "Lớp không có giới hạn số học sinh" 
                    : (cls.CurrentStudentsCount + studentIds.Count) <= cls.TotalStudents
                        ? $"Có thể thêm {studentIds.Count} học sinh"
                        : $"Không thể thêm {studentIds.Count} học sinh. Chỉ có thể thêm tối đa {cls.TotalStudents - cls.CurrentStudentsCount} học sinh nữa."
            };

            return Ok(result);
        }
    }
}