using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SaasAsaasApp.Data;
using SaasAsaasApp.Data.Interfaces;
using SaasAsaasApp.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace SaasAsaasApp.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ApplicationDbContext _context;

    public RegisterModel(IAuthService authService, ApplicationDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Plans { get; set; } = null!;

    public class InputModel
    {
        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required, Display(Name = "Tax ID (CPF/CNPJ)")]
        public string TaxId { get; set; } = string.Empty;

        [Required, Display(Name = "Select Plan")]
        public Guid PlanId { get; set; }
    }

    public async Task OnGetAsync()
    {
        var plans = await _context.SubscriptionPlans.Where(p => p.IsActive).ToListAsync();
        Plans = new SelectList(plans, "Id", "DisplayName");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var result = await _authService.RegisterTenantAsync(new RegisterRequest
        {
            FullName = Input.FullName,
            Email = Input.Email,
            Password = Input.Password,
            CompanyName = Input.CompanyName,
            TaxId = Input.TaxId,
            PlanId = Input.PlanId
        });

        if (result.Success)
        {
            // In a real scenario, we might sign in the user here or redirect to login
            // For now, let's redirect to Onboarding
            return RedirectToPage("/Billing/Onboarding");
        }

        ModelState.AddModelError(string.Empty, result.Message ?? "Registration failed.");
        await OnGetAsync();
        return Page();
    }
}
