using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JuiceShopDotNet.Unsafe.Data;

public partial class AspNetUser : IdentityUser
{
    //public string Id { get; set; } = null!;

    //public string? UserName { get; set; }

    //public string? NormalizedUserName { get; set; }

    //[Required]
    //public string? Email { get; set; }

    //public string? NormalizedEmail { get; set; }

    //public bool EmailConfirmed { get; set; }

    //public string? PasswordHash { get; set; }

    //public string? SecurityStamp { get; set; }

    //public string? ConcurrencyStamp { get; set; }

    //public string? PhoneNumber { get; set; }

    //public bool PhoneNumberConfirmed { get; set; }

    //public bool TwoFactorEnabled { get; set; }

    //public DateTimeOffset? LockoutEnd { get; set; }

    //public bool LockoutEnabled { get; set; }

    //public int AccessFailedCount { get; set; }

    [NotMapped]
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [NotMapped]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
}
