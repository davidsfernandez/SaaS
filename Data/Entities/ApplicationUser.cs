using Microsoft.AspNetCore.Identity;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Entities;

public class ApplicationUser : IdentityUser, ITenantEntity
{
    public Guid TenantId { get; set; }
    
    [PersonalData]
    public string? FullName { get; set; }
}
