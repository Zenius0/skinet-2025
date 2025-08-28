using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class AssignRoleDto
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
