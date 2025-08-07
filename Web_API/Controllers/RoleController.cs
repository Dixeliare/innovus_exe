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
    [Produces("application/json")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        // GET: api/Roles/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRoleById(int id)
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var role = await _roleService.GetByIdAsync(id);
            return Ok(role);
        }

        // POST: api/Roles
        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            var createdRole = await _roleService.AddAsync(createRoleDto);
            return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.RoleId }, createdRole);

        }

        // PUT: api/Roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            if (id != updateRoleDto.RoleId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "RoleId", new string[] { "ID vai trò trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            await _roleService.UpdateAsync(updateRoleDto);
            return NoContent();

        }

        // DELETE: api/Roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ApiException nếu có lỗi.
            await _roleService.DeleteAsync(id);
            return NoContent();
        }
    }
}
