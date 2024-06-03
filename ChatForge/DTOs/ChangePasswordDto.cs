using System.ComponentModel.DataAnnotations;

namespace ChatForge.DTOs;

public class ChangePasswordDto
{
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
}