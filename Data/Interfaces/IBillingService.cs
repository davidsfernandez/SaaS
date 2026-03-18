namespace SaasAsaasApp.Data.Interfaces;

public interface IBillingService
{
    Task<bool> CanChangePlanAsync(Guid tenantId);
    Task<bool> UpdateTenantPlanAsync(Guid tenantId, Guid newPlanId);
}
