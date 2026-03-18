using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SaasAsaasApp.Data.Enums;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Entities;

public class Tenant : IAuditableEntity, ISoftDeletable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? TaxId { get; set; } // Tax Identification (CPF/CNPJ for Asaas integration)

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public TenantStatus Status { get; set; } = TenantStatus.PendingPayment;

    // Asaas Integration
    [MaxLength(100)]
    public string? AsaasCustomerId { get; set; }

    // Relationship with Subscription Plan
    public Guid CurrentPlanId { get; set; }
    
    [ForeignKey("CurrentPlanId")]
    public SubscriptionPlan? CurrentPlan { get; set; }

    public DateTime? TrialEndsAt { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }

    // Dynamic Configuration (JSON)
    public string MetadataJson { get; set; } = "{}";

    // Audit and Soft Delete
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
