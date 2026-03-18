using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaasAsaasApp.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SaasAsaasApp.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly IAuthService _authService;

    public ForgotPasswordModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Message { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.ForgotPasswordAsync(Email);
        Message = result.Message;
        
        return Page();
    }
}
