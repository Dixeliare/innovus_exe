using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IUserRepository : IGenericRepository<user>
{
    Task<IEnumerable<user>> GetAllAsync();
    Task<user> GetByIdAsync(int id);
    Task<user?> GetByUsernameAsync(string username);
    // Task<user> AddAsync(user entity);
    // Task UpdateAsync(user entity);
    // Task<bool> DeleteAsync(int id);

    Task<IEnumerable<user>> SearchUsersAsync(
        string? username = null,
        string? accountName = null,
        string? password = null, // Giữ nguyên ở đây, nhưng logic bên dưới sẽ không dùng
        string? address = null,
        string? phoneNumber = null,
        bool? isDisabled = null,
        DateTime? createAt = null,
        DateOnly? birthday = null,
        int? roleId = null);
    
}