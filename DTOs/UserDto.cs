using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DTOs;

public class UserDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? AccountName { get; set; }
        // KHÔNG BAO GỒM MẬT KHẨU TRONG OUTPUT DTO VÌ LÝ DO BẢO MẬT!
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsDisabled { get; set; }
        public DateTime? CreateAt { get; set; }
        public string? AvatarUrl { get; set; }
        public DateOnly? Birthday { get; set; }
        public int? RoleId { get; set; }
        public int? StatisticId { get; set; }
        public int? OpeningScheduleId { get; set; }
        public int? ScheduleId { get; set; }
        
        public string? Email { get; set; }
        public int GenderId { get; set; } // ID của giới tính
        public GenderDto? Gender { get; set; } // Đối tượng Gender để hiển thị tên giới tính

        // Các khóa ngoại khác (nếu cần hiển thị chi tiết)
        public RoleDto? Role { get; set; }
        
        public List<int>? ClassIds { get; set; } 

        // Có thể thêm DTO lồng nhau cho các mối quan hệ Many-to-One nếu cần chi tiết
        // public RoleDto? Role { get; set; }
        // public StatisticDto? Statistic { get; set; }
        // public OpeningScheduleDto? OpeningSchedule { get; set; }
        // public ScheduleDto? Schedule { get; set; }
    }

    // DTO dùng làm input khi tạo mới Người dùng (POST request body)
    public class CreateUserDto
    {
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string? Username { get; set; }

        [StringLength(100, ErrorMessage = "Account Name cannot exceed 100 characters.")]
        public string? AccountName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters.")]
        public string Password { get; set; } = null!;

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? Address { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        public string? PhoneNumber { get; set; }

        public bool? IsDisabled { get; set; } = false;

        // [Url(ErrorMessage = "Invalid URL format for Avatar URL.")] // Bỏ validation này
        // [StringLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters.")]
        // public string? AvatarUrl { get; set; } // Bỏ thuộc tính này

        public IFormFile? AvatarImageFile { get; set; } // Thêm thuộc tính này để nhận file (có thể là null)

        public DateOnly? Birthday { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Gender ID is required.")]
        public int GenderId { get; set; }

        public int? RoleId { get; set; }
        public int? StatisticId { get; set; }
        public int? OpeningScheduleId { get; set; }
        public int? ScheduleId { get; set; }
        // THÊM TRƯỜNG NÀY ĐỂ NHẬN CLASS ID TỪ FRONTEND
        public int? ClassId { get; set; }
    }

    // DTO dùng làm input khi cập nhật Người dùng (PUT request body)
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "User ID is required for update.")]
        public int UserId { get; set; }

        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string? Username { get; set; }

        [StringLength(100, ErrorMessage = "Account Name cannot exceed 100 characters.")]
        public string? AccountName { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters.")]
        public string? NewPassword { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? Address { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        public string? PhoneNumber { get; set; }

        public bool? IsDisabled { get; set; }

        // [Url(ErrorMessage = "Invalid URL format for Avatar URL.")] // Bỏ validation này
        // [StringLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters.")]
        // public string? AvatarUrl { get; set; } // Bỏ thuộc tính này

        public IFormFile? AvatarImageFile { get; set; } // Thêm thuộc tính này để nhận file (có thể là null)

        public DateOnly? Birthday { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string? Email { get; set; } // Email có thể là null khi cập nhật nếu không muốn thay đổi

        [Required(ErrorMessage = "Gender ID is required.")] // Thêm Required nếu luôn bắt buộc
        public int GenderId { get; set; }

        public int? RoleId { get; set; }
        public int? StatisticId { get; set; }
        public int? OpeningScheduleId { get; set; }
        public int? ScheduleId { get; set; }
    }