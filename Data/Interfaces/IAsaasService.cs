using SaasAsaasApp.Models.Asaas;

namespace SaasAsaasApp.Data.Interfaces;

public interface IAsaasService
{
    Task<AsaasCustomerResponse?> CreateCustomerAsync(AsaasCustomerRequest request);
    Task<AsaasPaymentResponse?> CreatePaymentAsync(AsaasPaymentRequest request);
    Task<AsaasPaymentResponse?> GetPaymentAsync(string paymentId);
    Task<AsaasPaymentListResponse?> ListPaymentsAsync(string customerId);
    Task<AsaasPaymentResponse?> UpdateSubscriptionAsync(string subscriptionId, decimal newValue);
}
