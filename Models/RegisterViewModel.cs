using System.ComponentModel.DataAnnotations;

namespace ECL.Models;

public sealed class RegisterViewModel
{
    [Required]
    [StringLength(32, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

