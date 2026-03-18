using SaasAsaasApp.Data.Entities;

namespace SaasAsaasApp.Data.Interfaces;

public interface IProjectService : IBaseService<Project>
{
    Task<IEnumerable<Project>> GetAllWithCustomerAsync();
}
