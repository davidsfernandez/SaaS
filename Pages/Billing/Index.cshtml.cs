using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SaasAsaasApp.Data;
using SaasAsaasApp.Data.Entities;
using SaasAsaasApp.Data.Interfaces;
using SaasAsaasApp.Models.Asaas;

namespace SaasAsaasApp.Pages.Billing;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IBillingService _billingService;
    private readonly IAsaasService _asaasService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        ApplicationDbContext context,
        IBillingService billingService,
        IAsaasService asaasService,
        ITenantProvider tenantProvider,
        ILogger<IndexModel> logger)
    {
        _context = context;
        _billingService = billingService;
        _asaasService = asaasService;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public Tenant Tenant { get; set; } = default!;
    public SubscriptionPlan CurrentPlan { get; set; } = default!;
    public List<SubscriptionPlan> AvailablePlans { get; set; } = new();
    public AsaasPaymentListResponse? PaymentHistory { get; set; }
    public bool CanChangePlan { get; set; }
    public bool IsApiError { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var tenantId = _tenantProvider.TenantId;
        if (tenantId == Guid.Empty)
        {
            return RedirectToPage("/Index");
        }

        var tenant = await _context.Tenants
            .Include(t => t.CurrentPlan)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            return NotFound("Tenant not found.");
        }

        Tenant = tenant;
        
        if (tenant.CurrentPlan == null)
        {
            return NotFound("Current subscription plan not found.");
        }

        CurrentPlan = tenant.CurrentPlan;
        
        AvailablePlans = await _context.SubscriptionPlans
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.Price)
            .ToListAsync();

        CanChangePlan = await _billingService.CanChangePlanAsync(tenantId);

        if (!string.IsNullOrEmpty(tenant.AsaasCustomerId))
        {
            PaymentHistory = await _asaasService.ListPaymentsAsync(tenant.AsaasCustomerId);
            if (PaymentHistory == null)
            {
                IsApiError = true;
                _logger.LogWarning("Asaas API returned null for payment history of tenant {TenantId}", tenantId);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostChangePlanAsync(Guid planId)
    {
        var tenantId = _tenantProvider.TenantId;
        if (tenantId == Guid.Empty)
        {
            return RedirectToPage("/Index");
        }

        var success = await _billingService.UpdateTenantPlanAsync(tenantId, planId);

        if (success)
        {
            StatusMessage = "Your subscription plan has been updated successfully.";
        }
        else
        {
            StatusMessage = "Error: Could not update your subscription plan. Please check for overdue payments or contact support.";
        }

        return RedirectToPage();
    }
}
