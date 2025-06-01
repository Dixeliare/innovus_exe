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
using Services.IServices;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        // POST: api/Roles
        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            try
            {
                var createdRole = await _roleService.AddAsync(createRoleDto);
                return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.RoleId }, createdRole);
            }
            catch (Exception ex)
            {
                // Vì không có khóa ngoại, lỗi thường là do validation hoặc DB
                return StatusCode(500, new { message = "An error occurred while creating the role.", error = ex.Message });
            }
        }

        // PUT: api/Roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            if (id != updateRoleDto.RoleId)
            {
                return BadRequest(new { message = "Role ID in URL does not match ID in body." });
            }

            try
            {
                await _roleService.UpdateAsync(updateRoleDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the role.", error = ex.Message });
            }
        }

        // DELETE: api/Roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                await _roleService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the role.", error = ex.Message });
            }
        }
    }
}
