using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SaasAsaasApp.Data.Enums;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Services;

public class TenantStatusService : ITenantStatusService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TenantStatusService> _logger;
    private const string CacheKeyPrefix = "tenant_status_";

    public TenantStatusService(ApplicationDbContext context, IMemoryCache cache, ILogger<TenantStatusService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TenantStatus> GetTenantStatusAsync(Guid tenantId)
    {
        // Try to get status from cache for high performance
        if (_cache.TryGetValue(CacheKeyPrefix + tenantId, out TenantStatus status))
        {
            return status;
        }

        // Fallback to database if not in cache
        var tenant = await _context.Tenants
            .IgnoreQueryFilters() // Bypass multi-tenant filter to see the tenant itself
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            return TenantStatus.Canceled;
        }

        // Cache the status for 5 minutes (or until webhook invalidates it)
        _cache.Set(CacheKeyPrefix + tenantId, tenant.Status, TimeSpan.FromMinutes(5));
        
        return tenant.Status;
    }

    public async Task<bool> IsTenantActiveAsync(Guid tenantId)
    {
        var status = await GetTenantStatusAsync(tenantId);
        return status == TenantStatus.Active;
    }

    public async Task UpdateTenantStatusAsync(Guid tenantId, TenantStatus newStatus)
    {
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant != null)
        {
            tenant.Status = newStatus;
            await _context.SaveChangesAsync();

            // Invalidate/Update cache immediately for real-time revocation
            _cache.Set(CacheKeyPrefix + tenantId, newStatus);
            _logger.LogInformation("Tenant {TenantId} status updated to {Status} and cache invalidated", tenantId, newStatus);
        }
    }
}
