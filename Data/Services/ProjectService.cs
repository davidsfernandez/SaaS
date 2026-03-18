using Microsoft.EntityFrameworkCore;
using SaasAsaasApp.Data.Entities;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Services;

public class ProjectService : BaseService<Project>, IProjectService
{
    public ProjectService(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Project>> GetAllWithCustomerAsync()
    {
        return await _dbSet
            .Include(p => p.Customer)
            .ToListAsync();
    }
}
