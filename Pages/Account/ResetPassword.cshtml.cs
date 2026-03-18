using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaasAsaasApp.Data.Interfaces;
using SaasAsaasApp.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace SaasAsaasApp.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly IAuthService _authService;

    public ResetPasswordModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? token = null, string? email = null)
    {
        if (token == null || email == null)
        {
            // Handle invalid request
        }
        else
        {
            Input.Token = token;
            Input.Email = email;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.ResetPasswordAsync(new ResetPasswordRequest
        {
            Email = Input.Email,
            Token = Input.Token,
            NewPassword = Input.Password
        });

        if (result.Success)
        {
            return RedirectToPage("./Login", new { ErrorMessage = "Password reset successful. Please login with your new password." });
        }

        ModelState.AddModelError(string.Empty, result.Message ?? "Error resetting password.");
        return Page();
    }
}
