using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class RoleService : IRoleService
    {
        private readonly RoleRepository _roleRepository;

        public RoleService(RoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        // Mapper từ Model sang DTO
        private RoleDto MapToRoleDto(role model)
        {
            return new RoleDto
            {
                RoleId = model.role_id,
                RoleName = model.role_name
            };
        }

        // GET All Roles
        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(MapToRoleDto);
        }

        // GET Role by ID
        public async Task<RoleDto?> GetByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            return role != null ? MapToRoleDto(role) : null;
        }

        // CREATE Role
        public async Task<RoleDto> AddAsync(CreateRoleDto createRoleDto)
        {
            var roleEntity = new role
            {
                role_name = createRoleDto.RoleName
            };

            var addedRole = await _roleRepository.AddAsync(roleEntity);
            return MapToRoleDto(addedRole);
        }

        // UPDATE Role
        public async Task UpdateAsync(UpdateRoleDto updateRoleDto)
        {
            var existingRole = await _roleRepository.GetByIdAsync(updateRoleDto.RoleId);

            if (existingRole == null)
            {
                throw new KeyNotFoundException($"Role with ID {updateRoleDto.RoleId} not found.");
            }

            // Cập nhật tên nếu có giá trị được cung cấp
            if (!string.IsNullOrEmpty(updateRoleDto.RoleName))
            {
                existingRole.role_name = updateRoleDto.RoleName;
            }
            // Nếu bạn muốn cho phép gán null cho tên vai trò (nếu DB cho phép), bạn có thể thêm:
            // else if (updateRoleDto.RoleName == null)
            // {
            //     existingRole.role_name = null;
            // }

            await _roleRepository.UpdateAsync(existingRole);
        }

        // DELETE Role
        public async Task DeleteAsync(int id)
        {
            await _roleRepository.DeleteAsync(id);
        }
    }