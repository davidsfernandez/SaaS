using Microsoft.EntityFrameworkCore;
using SaasAsaasApp.Data.Interfaces;
using SaasAsaasApp.Models.Dashboard;
using SaasAsaasApp.Data.Enums;

namespace SaasAsaasApp.Data.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public DashboardService(ApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<DashboardMetrics> GetTenantMetricsAsync()
    {
        var tenantId = _tenantProvider.TenantId;

        // Get Tenant Info (Ignoring global filter to see the tenant record itself)
        var tenant = await _context.Tenants
            .Include(t => t.CurrentPlan)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        var metrics = new DashboardMetrics
        {
            TenantName = tenant?.Name ?? "Unknown Company",
            SubscriptionStatus = tenant?.Status.ToString() ?? "Pending",
            PlanName = tenant?.CurrentPlan?.DisplayName ?? "No Plan",
            TotalCustomers = await _context.Customers.CountAsync(),
            TotalProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Active || p.Status == ProjectStatus.Paused),
            TotalEstimatedValue = await _context.Projects.Where(p => p.Status != ProjectStatus.Canceled).SumAsync(p => p.EstimatedValue)
        };

        return metrics;
    }
}
