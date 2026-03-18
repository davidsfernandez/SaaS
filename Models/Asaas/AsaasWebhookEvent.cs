namespace SaasAsaasApp.Models.Asaas;

public class AsaasWebhookEvent
{
    public string Event { get; set; } = string.Empty;
    public AsaasWebhookPayment Payment { get; set; } = new();
}

public class AsaasWebhookPayment
{
    public string Id { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
}
