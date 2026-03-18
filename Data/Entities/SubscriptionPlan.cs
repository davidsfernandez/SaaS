using System.ComponentModel.DataAnnotations;
using SaasAsaasApp.Data.Enums;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Entities;

public class SubscriptionPlan : IAuditableEntity, ISoftDeletable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string InternalName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "BRL";

    public BillingCycle BillingCycle { get; set; }

    public bool IsActive { get; set; } = true;

    // Limits (Quotas)
    public int MaxUsers { get; set; }
    public int MaxProjects { get; set; }
    
    // Feature Flags (JSON)
    public string FeaturesJson { get; set; } = "{}";

    // Asaas Integration
    [MaxLength(100)]
    public string? AsaasPlanId { get; set; }

    // Audit and Soft Delete
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
