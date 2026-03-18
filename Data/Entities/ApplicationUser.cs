using Microsoft.AspNetCore.Identity;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Entities;

public class ApplicationUser : IdentityUser, ITenantEntity, ISoftDeletable
{
    public Guid TenantId { get; set; }
    
    [PersonalData]
    public string? FullName { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
