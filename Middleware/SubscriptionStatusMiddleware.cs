using SaasAsaasApp.Data.Enums;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Middleware;

public class SubscriptionStatusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SubscriptionStatusMiddleware> _logger;

    public SubscriptionStatusMiddleware(RequestDelegate next, ILogger<SubscriptionStatusMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider, ITenantStatusService statusService)
    {
        // 1. Skip check for non-authenticated users or public endpoints
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // 2. Skip check for specific public/essential paths
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // 3. Get TenantId and check status
        var tenantId = tenantProvider.TenantId;
        if (tenantId == Guid.Empty)
        {
            await _next(context);
            return;
        }

        var status = await statusService.GetTenantStatusAsync(tenantId);

        // 4. Handle Access Control based on Status
        if (status != TenantStatus.Active)
        {
            _logger.LogWarning("Access denied for Tenant {TenantId} with status {Status}", tenantId, status);

            if (status == TenantStatus.PendingPayment && !path.Contains("/billing/onboarding"))
            {
                context.Response.Redirect("/Billing/Onboarding");
                return;
            }

            if (status == TenantStatus.Overdue && !path.Contains("/billing/payment"))
            {
                // In an MVP, we might allow read access or just redirect to payment
                context.Response.Redirect("/Billing/PaymentRequired");
                return;
            }

            if (status == TenantStatus.Canceled)
            {
                context.Response.Redirect("/Account/AccessDenied");
                return;
            }
        }

        await _next(context);
    }

    private bool IsPublicPath(string path)
    {
        return path.StartsWith("/api/webhooks") || 
               path.StartsWith("/account/") || 
               path.StartsWith("/billing/onboarding") ||
               path.StartsWith("/billing/payment") ||
               path.Contains("/assets/") ||
               path.Contains("/error") ||
               path.Contains("/lib/") ||
               path.Contains("/css/") ||
               path.Contains("/js/");
    }
}
