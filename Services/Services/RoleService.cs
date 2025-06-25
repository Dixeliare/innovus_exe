using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class RoleService : IRoleService
{
    // private readonly IRoleRepository _roleRepository;
    //
    // public RoleService(IRoleRepository roleRepository)
    // {
    //     _roleRepository = roleRepository;
    // }
    
    private readonly IUnitOfWork _unitOfWork; // <-- Thêm dòng này

    public RoleService(IUnitOfWork unitOfWork) // <-- Sửa constructor
    {
        _unitOfWork = unitOfWork;
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
        var roles = await _unitOfWork.Roles.GetAllAsync(); // <-- Sử dụng UnitOfWork
        return roles.Select(MapToRoleDto);
    }

    // GET Role by ID
    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id); // <-- Sử dụng UnitOfWork
        if (role == null)
        {
            throw new NotFoundException("Role", "Id", id);
        }
        return MapToRoleDto(role);
    }

    // CREATE Role
    public async Task<RoleDto> AddAsync(CreateRoleDto createRoleDto)
    {
        // Kiểm tra trùng tên vai trò
        var existingRole = await _unitOfWork.Roles.FindOneAsync(r => r.role_name == createRoleDto.RoleName); // Giả định FindOneAsync hoặc một phương thức tương tự tồn tại
        if (existingRole != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "RoleName", new string[] { $"Tên vai trò '{createRoleDto.RoleName}' đã tồn tại." } }
            });
        }

        var roleEntity = new role
        {
            role_name = createRoleDto.RoleName
        };

        try
        {
            var addedRole = await _unitOfWork.Roles.AddAsync(roleEntity); // <-- Sử dụng UnitOfWork
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
            return MapToRoleDto(addedRole);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm vai trò vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the role.", ex, (int)HttpStatusCode.InternalServerError);
        }

    }

    // UPDATE Role
    public async Task UpdateAsync(UpdateRoleDto updateRoleDto)
    {
        var existingRole = await _unitOfWork.Roles.GetByIdAsync(updateRoleDto.RoleId); // <-- Sử dụng UnitOfWork

        if (existingRole == null)
        {
            throw new NotFoundException("Role", "Id", updateRoleDto.RoleId);
        }

        // Kiểm tra trùng tên vai trò nếu tên mới được cung cấp và khác với tên cũ
        if (!string.IsNullOrEmpty(updateRoleDto.RoleName) && updateRoleDto.RoleName != existingRole.role_name)
        {
            var roleWithSameName = await _unitOfWork.Roles.FindOneAsync(r => r.role_name == updateRoleDto.RoleName);
            if (roleWithSameName != null && roleWithSameName.role_id != updateRoleDto.RoleId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "RoleName", new string[] { $"Tên vai trò '{updateRoleDto.RoleName}' đã được sử dụng bởi một vai trò khác." } }
                });
            }
        }

        // Cập nhật tên nếu có giá trị được cung cấp
        if (updateRoleDto.RoleName != null) // Cho phép gán null nếu DTO cho phép và DB cho phép
        {
            existingRole.role_name = updateRoleDto.RoleName;
        }

        try
        {
            await _unitOfWork.Roles.UpdateAsync(existingRole); // <-- Sử dụng UnitOfWork
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật vai trò trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the role.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // DELETE Role
    public async Task DeleteAsync(int id)
    {
        var roleToDelete = await _unitOfWork.Roles.GetByIdAsync(id); // <-- Sử dụng UnitOfWork
        if (roleToDelete == null)
        {
            throw new NotFoundException("Role", "Id", id);
        }

        try
        {
            await _unitOfWork.Roles.DeleteAsync(id); // <-- Sử dụng UnitOfWork
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu có user nào đó đang liên kết với vai trò này, sẽ ném lỗi FK
            throw new ApiException("Không thể xóa vai trò này vì nó đang được sử dụng bởi một hoặc nhiều người dùng.", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the role.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }
}