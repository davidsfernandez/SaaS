namespace SaasAsaasApp.Models.Asaas;

public class AsaasCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CpfCnpj { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
}

public class AsaasCustomerResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class AsaasPaymentRequest
{
    public string Customer { get; set; } = string.Empty; // Customer ID from Asaas
    public string BillingType { get; set; } = "UNDEFINED"; // BOLETO, CREDIT_CARD, PIX, UNDEFINED
    public decimal Value { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
}

public class AsaasPaymentResponse
{
    public string Id { get; set; } = string.Empty;
    public string InvoiceUrl { get; set; } = string.Empty;
    public string BankSlipUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
