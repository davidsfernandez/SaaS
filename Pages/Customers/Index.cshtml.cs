using Microsoft.AspNetCore.Mvc.RazorPages;
using SaasAsaasApp.Data.Entities;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Pages.Customers;

public class IndexModel : PageModel
{
    private readonly IBaseService<Customer> _customerService;

    public IndexModel(IBaseService<Customer> customerService)
    {
        _customerService = customerService;
    }

    public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();

    public async Task OnGetAsync()
    {
        // This will only return customers belonging to the current Tenant
        // thanks to the Global Query Filter in ApplicationDbContext.
        Customers = await _customerService.GetAllAsync();
    }
}
