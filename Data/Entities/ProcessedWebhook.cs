using System.ComponentModel.DataAnnotations;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Entities;

public class ProcessedWebhook : IAuditableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string EventId { get; set; } = string.Empty; // Asaas unique event ID

    [Required, MaxLength(100)]
    public string PaymentId { get; set; } = string.Empty;

    public string? EventType { get; set; }
    public string? RawData { get; set; }

    // Auditing
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
