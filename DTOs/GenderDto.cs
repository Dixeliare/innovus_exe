using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class GenderDto
{
    public int GenderId { get; set; }
    public string? GenderName { get; set; }
}

public class CreateGenderDto
{
    [Required(ErrorMessage = "Gender name is required.")]
    [StringLength(50, ErrorMessage = "Gender name cannot exceed 50 characters.")]
    public string GenderName { get; set; } = null!;
}

public class UpdateGenderDto
{
    [Required(ErrorMessage = "Gender ID is required for update.")]
    public int GenderId { get; set; }

    [Required(ErrorMessage = "Gender name is required.")]
    [StringLength(50, ErrorMessage = "Gender name cannot exceed 50 characters.")]
    public string GenderName { get; set; } = null!;
}