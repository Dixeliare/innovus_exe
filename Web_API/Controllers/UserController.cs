using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<user> GetByIdAsync(int id)
        {
            return await _userService.GetByIdAsync(id);
        }

        [HttpGet("search_by")]
        public async Task<IEnumerable<user>> SearchByUserAsync(
            [FromQuery] string? username = null,
            [FromQuery] string? accountName = null,
            [FromQuery] string? password = null,
            [FromQuery] string? address = null,
            [FromQuery] string? phoneNumber = null,
            [FromQuery] bool? isDisabled = null,
            [FromQuery] DateTime? createAt = null,
            [FromQuery] DateOnly? birthday = null,
            [FromQuery] int? roleId = null)
        {
            return await _userService.SearchUsersAsync(username, accountName, password, address, phoneNumber, isDisabled, createAt, birthday, roleId);
        }

        [HttpPost]
        public async Task<int> PostAsync(user user)
        {
            return await _userService.CreateAsync(user);
        }

        [HttpPut]
        public async Task<int> PutAsync(user user)
        {
            return await _userService.UpdateAsync(user);
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(int id)
        {
            return await _userService.DeleteAsync(id);
        }
    }
}
