using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SaasAsaasApp.Data.Entities;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Pages.Projects;

public class EditModel : PageModel
{
    private readonly IProjectService _projectService;
    private readonly IBaseService<Customer> _customerService;

    public EditModel(IProjectService projectService, IBaseService<Customer> customerService)
    {
        _projectService = projectService;
        _customerService = customerService;
    }

    [BindProperty]
    public Project Project { get; set; } = null!;

    public SelectList CustomerList { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        Project = project;
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

        // Safety check: ensure the project exists and belongs to the tenant
        var existingProject = await _projectService.GetByIdAsync(Project.Id);
        if (existingProject == null)
        {
            return NotFound();
        }

        // Update fields
        existingProject.Title = Project.Title;
        existingProject.Description = Project.Description;
        existingProject.Status = Project.Status;
        existingProject.EstimatedValue = Project.EstimatedValue;
        existingProject.StartDate = Project.StartDate;
        existingProject.EndDate = Project.EndDate;
        existingProject.CustomerId = Project.CustomerId;

        await _projectService.UpdateAsync(existingProject);

        return RedirectToPage("./Index");
    }

    private async Task PopulateCustomerListAsync()
    {
        var customers = await _customerService.GetAllAsync();
        CustomerList = new SelectList(customers, "Id", "Name");
    }
}
