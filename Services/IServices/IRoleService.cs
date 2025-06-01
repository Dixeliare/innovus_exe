using DTOs;

namespace Services.IServices;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<RoleDto?> GetByIdAsync(int id);
    Task<RoleDto> AddAsync(CreateRoleDto createRoleDto);
    Task UpdateAsync(UpdateRoleDto updateRoleDto);
    Task DeleteAsync(int id);
}