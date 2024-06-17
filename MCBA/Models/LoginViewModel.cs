using Microsoft.Build.Framework;

namespace MCBA.Models;

public class LoginViewModel
{
    [Required]
    public string LoginID { get; set; }

    [Required]
    public string PasswordHash { get; set; }
}