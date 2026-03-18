namespace SaasAsaasApp.Models.Dashboard;

public class DashboardMetrics
{
    public int TotalCustomers { get; set; }
    public int TotalProjects { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
}
