using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IUserService
{
    Task<IEnumerable<user>> GetAllAsync();
    Task<user> GetByIdAsync(int id);
    Task<UserDto?> GetByUsernameAsync(string username); 
    Task<UserDto> AddAsync(CreateUserDto createUserDto);
    Task UpdateAsync(UpdateUserDto updateUserDto);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<user>> SearchUsersAsync(
        string? username = null,
        string? accountName = null,
        string? password = null,
        string? address = null,
        string? phoneNumber = null,
        bool? isDisabled = null,
        DateTime? createAt = null,
        DateOnly? birthday = null,
        int? roleId = null);
}