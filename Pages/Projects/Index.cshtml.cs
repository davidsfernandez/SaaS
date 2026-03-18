using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaasAsaasApp.Data.Entities;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Pages.Projects;

public class IndexModel : PageModel
{
    private readonly IProjectService _projectService;

    public IndexModel(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public IEnumerable<Project> Projects { get; set; } = new List<Project>();

    public async Task OnGetAsync()
    {
        Projects = await _projectService.GetAllWithCustomerAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _projectService.DeleteAsync(id);
        return RedirectToPage();
    }
}
