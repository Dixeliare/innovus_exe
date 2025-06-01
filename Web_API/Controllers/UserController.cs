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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        
        public UserController(IUserService userService) => _userService = userService;

        [HttpGet]
        public async Task<IEnumerable<user>> GetAllAsync()
        {
            return await _userService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // GET: api/Users/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }


        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var createdUser = await _userService.AddAsync(createUserDto);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.UserId }, createdUser);
            }
            catch (ArgumentException ex) // Bắt lỗi trùng lặp username
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex) // Bắt lỗi khóa ngoại
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the user.", error = ex.Message });
            }
        }

        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (id != updateUserDto.UserId)
            {
                return BadRequest(new { message = "User ID in URL does not match ID in body." });
            }

            try
            {
                await _userService.UpdateAsync(updateUserDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex) // Bắt lỗi trùng lặp username
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user.", error = ex.Message });
            }
        }

        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user.", error = ex.Message });
            }
        }
    }
}
