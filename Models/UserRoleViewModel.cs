using System.ComponentModel.DataAnnotations;

namespace SmartTaskTracker.Models;

public class UserRoleViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    [Required]
    public string Role { get; set; } = string.Empty;
}
