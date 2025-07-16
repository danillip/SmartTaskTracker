using System.ComponentModel.DataAnnotations;

namespace SmartTaskTracker.Models;

public class UserCreateViewModel
{
    [Required]
    public string UserName { get; set; } = string.Empty;
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string Role { get; set; } = string.Empty;
}
