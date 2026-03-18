using System.Security.Claims;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;
            
            if (Guid.TryParse(tenantClaim, out var tenantId))
            {
                return tenantId;
            }

            return Guid.Empty;
        }
    }
}
