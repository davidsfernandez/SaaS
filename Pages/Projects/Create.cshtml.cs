using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SaasAsaasApp.Data.Entities;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Pages.Projects;

public class CreateModel : PageModel
{
    private readonly IProjectService _projectService;
    private readonly IBaseService<Customer> _customerService;

    public CreateModel(IProjectService projectService, IBaseService<Customer> customerService)
    {
        _projectService = projectService;
        _customerService = customerService;
    }

    [BindProperty]
    public Project Project { get; set; } = new();

    public SelectList CustomerList { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        await PopulateCustomerListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateCustomerListAsync();
            return Page();
        }

        await _projectService.CreateAsync(Project);

        return RedirectToPage("./Index");
    }

    private async Task PopulateCustomerListAsync()
    {
        var customers = await _customerService.GetAllAsync();
        CustomerList = new SelectList(customers, "Id", "Name");
    }
}
