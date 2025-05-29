using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class UserService : IUserService
{
    private readonly UserRepository _userRepository;
    
    public UserService(UserRepository userRepository) => _userRepository = userRepository;
    
    public async Task<IEnumerable<user>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<user> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(user user)
    {
        return await _userRepository.CreateAsync(user);
    }

    public async Task<int> UpdateAsync(user user)
    {
        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<user>> SearchUsersAsync(string? username = null, string? accountName = null, string? password = null,
        string? address = null, string? phoneNumber = null, bool? isDisabled = null, DateTime? createAt = null,
        DateOnly? birthday = null, int? roleId = null)
    {
        return await _userRepository.SearchUsersAsync(username, accountName, password, address, phoneNumber, isDisabled, createAt, birthday, roleId);
    }
}