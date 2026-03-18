using Microsoft.EntityFrameworkCore;
using SaasAsaasApp.Data.Enums;

namespace SaasAsaasApp.Data.Interfaces;

public interface ITenantStatusService
{
    Task<TenantStatus> GetTenantStatusAsync(Guid tenantId);
    Task<bool> IsTenantActiveAsync(Guid tenantId);
    Task UpdateTenantStatusAsync(Guid tenantId, TenantStatus newStatus);
}
