using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SaasAsaasApp.Data;
using SaasAsaasApp.Data.Enums;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Pages.Billing;

public class OnboardingModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public OnboardingModel(ApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public string CompanyName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // For simulation purposes in MVP, we look for the tenant
        // In a real scenario, the user would be authenticated and ITenantProvider would work.
        // If not authenticated, we redirect to login.
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage("/Account/Login");
        }

        var tenantId = _tenantProvider.TenantId;
        var tenant = await _context.Tenants
            .Include(t => t.CurrentPlan)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null) return RedirectToPage("/Account/Register");

        if (tenant.Status == TenantStatus.Active)
        {
            return RedirectToPage("/Index");
        }

        CompanyName = tenant.Name;
        PlanName = tenant.CurrentPlan?.DisplayName ?? "Standard Plan";
        Amount = tenant.CurrentPlan?.Price ?? 0;

        return Page();
    }
}
