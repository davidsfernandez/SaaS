using System.ComponentModel.DataAnnotations;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Entities;

public class Customer : ITenantEntity, IAuditableEntity, ISoftDeletable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TenantId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? Document { get; set; } // CPF/CNPJ of the end-customer

    // Audit and Soft Delete
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
