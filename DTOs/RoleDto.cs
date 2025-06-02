using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class RoleDto
{
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
}

// DTO dùng làm input khi tạo mới Vai trò (POST request body)
public class CreateRoleDto
{
    // Thường thì tên vai trò không nên null và nên là duy nhất
    [Required(ErrorMessage = "Role Name is required.")]
    [StringLength(50, ErrorMessage = "Role Name cannot exceed 50 characters.")]
    public string RoleName { get; set; } = null!;
}

// DTO dùng làm input khi cập nhật Vai trò (PUT request body)
public class UpdateRoleDto
{
    [Required(ErrorMessage = "Role ID is required for update.")]
    public int RoleId { get; set; }

    [StringLength(50, ErrorMessage = "Role Name cannot exceed 50 characters.")]
    public string? RoleName { get; set; } // Có thể null khi update nếu client không muốn thay đổi tên
}