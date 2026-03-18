using SaasAsaasApp.Models.Dashboard;

namespace SaasAsaasApp.Data.Interfaces;

public interface IDashboardService
{
    Task<DashboardMetrics> GetTenantMetricsAsync();
}
