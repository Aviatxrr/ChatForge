using System.ComponentModel.DataAnnotations;

namespace ChatForge.DTOs;

public class AuthenticationDto
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
}