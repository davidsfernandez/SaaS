using Microsoft.EntityFrameworkCore;
using SaasAsaasApp.Data.Interfaces;
using System.Text.Json;

namespace SaasAsaasApp.Data.Services;

public class BillingService : IBillingService
{
    private readonly ApplicationDbContext _context;
    private readonly IAsaasService _asaasService;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        ApplicationDbContext context,
        IAsaasService asaasService,
        ILogger<BillingService> logger)
    {
        _context = context;
        _asaasService = asaasService;
        _logger = logger;
    }

    public async Task<bool> CanChangePlanAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(tenant.AsaasCustomerId))
        {
            // If no Asaas customer yet, we assume they can change plan (e.g. during onboarding)
            return true;
        }

        var payments = await _asaasService.ListPaymentsAsync(tenant.AsaasCustomerId);
        if (payments == null)
        {
            _logger.LogWarning("Could not retrieve payments for tenant {TenantId} from Asaas.", tenantId);
            return false;
        }

        // Check if any payment is OVERDUE
        return !payments.Data.Any(p => p.Status == "OVERDUE");
    }

    public async Task<bool> UpdateTenantPlanAsync(Guid tenantId, Guid newPlanId)
    {
        // 1. Validate CanChangePlan
        if (!await CanChangePlanAsync(tenantId))
        {
            _logger.LogWarning("Tenant {TenantId} cannot change plan due to overdue payments or other restrictions.", tenantId);
            return false;
        }

        // 2. Get new plan details from DB
        var newPlan = await _context.SubscriptionPlans.FindAsync(newPlanId);
        if (newPlan == null)
        {
            _logger.LogError("Subscription plan {PlanId} not found.", newPlanId);
            return false;
        }

        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            return false;
        }

        // 3. Call AsaasService.UpdateSubscriptionAsync if applicable
        // We look for a subscription ID in MetadataJson
        string? subscriptionId = null;
        try
        {
            var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(tenant.MetadataJson);
            if (metadata != null && metadata.TryGetValue("AsaasSubscriptionId", out var id))
            {
                subscriptionId = id;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing MetadataJson for tenant {TenantId}", tenantId);
        }

        if (!string.IsNullOrEmpty(subscriptionId))
        {
            var updateResult = await _asaasService.UpdateSubscriptionAsync(subscriptionId, newPlan.Price);
            if (updateResult == null)
            {
                _logger.LogError("Failed to update subscription in Asaas for tenant {TenantId}.", tenantId);
                return false;
            }
        }

        // 4. Update local Tenant.CurrentPlanId
        tenant.CurrentPlanId = newPlanId;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
