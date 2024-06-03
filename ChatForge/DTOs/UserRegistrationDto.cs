using System.ComponentModel.DataAnnotations;

namespace ChatForge.DTOs;

public class UserRegistrationDto
{
    [Required]
    [StringLength(20, MinimumLength = 4)]
    public string Username { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
}