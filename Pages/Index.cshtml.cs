using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaasAsaasApp.Data.Interfaces;
using SaasAsaasApp.Models.Dashboard;

namespace SaasAsaasApp.Pages;

public class IndexModel : PageModel
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IDashboardService dashboardService, ILogger<IndexModel> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public DashboardMetrics Metrics { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            // If not authenticated, we just show the landing part of the index
            return Page();
        }

        try
        {
            Metrics = await _dashboardService.GetTenantMetricsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard metrics");
        }

        return Page();
    }
}
