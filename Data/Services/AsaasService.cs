using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SaasAsaasApp.Configurations;
using SaasAsaasApp.Data.Interfaces;
using SaasAsaasApp.Models.Asaas;

namespace SaasAsaasApp.Data.Services;

public class AsaasService : IAsaasService
{
    private readonly HttpClient _httpClient;
    private readonly AsaasSettings _settings;
    private readonly ILogger<AsaasService> _logger;

    public AsaasService(HttpClient httpClient, IOptions<AsaasSettings> settings, ILogger<AsaasService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AsaasCustomerResponse?> CreateCustomerAsync(AsaasCustomerRequest request)
    {
        try
        {
            PrepareHeaders();
            var json = JsonSerializer.Serialize(request, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/customers", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Asaas API error (CreateCustomer): {Error}", error);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AsaasCustomerResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling Asaas API (CreateCustomer)");
            return null;
        }
    }

    public async Task<AsaasPaymentResponse?> CreatePaymentAsync(AsaasPaymentRequest request)
    {
        try
        {
            PrepareHeaders();
            var json = JsonSerializer.Serialize(request, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/payments", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Asaas API error (CreatePayment): {Error}", error);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AsaasPaymentResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling Asaas API (CreatePayment)");
            return null;
        }
    }

    public async Task<AsaasPaymentResponse?> GetPaymentAsync(string paymentId)
    {
        try
        {
            PrepareHeaders();
            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/payments/{paymentId}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Asaas API error (GetPayment): {Error}", error);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AsaasPaymentResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling Asaas API (GetPayment)");
            return null;
        }
    }

    private void PrepareHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("access_token", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }
}
