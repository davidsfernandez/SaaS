using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SaasAsaasApp.Configurations;
using SaasAsaasApp.Data;
using SaasAsaasApp.Data.Entities;
using SaasAsaasApp.Data.Enums;
using SaasAsaasApp.Data.Interfaces;
using SaasAsaasApp.Models.Asaas;
using System.Text.Json;

namespace SaasAsaasApp.Controllers;

[ApiController]
[Route("api/webhooks/asaas")]
[EnableRateLimiting("StrictPolicy")]
public class AsaasWebhookController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly AsaasSettings _settings;
    private readonly ILogger<AsaasWebhookController> _logger;

    public AsaasWebhookController(
        ApplicationDbContext context, 
        IEmailService emailService,
        IOptions<AsaasSettings> settings, 
        ILogger<AsaasWebhookController> logger)
    {
        _context = context;
        _emailService = emailService;
        _settings = settings.Value;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] JsonElement rawEvent)
    {
        // 1. Security Validation: Check Webhook Access Token
        // The 'asaas-access-token' header is configured in the Asaas dashboard
        // and must match the WebhookToken in our configuration to ensure the request
        // is coming from a trusted source.
        if (string.IsNullOrEmpty(_settings.WebhookToken))
        {
            _logger.LogCritical("Asaas Webhook Token is not configured in application settings.");
            return StatusCode(500, "Webhook security configuration missing.");
        }

        if (!Request.Headers.TryGetValue("asaas-access-token", out var receivedToken) || 
            receivedToken != _settings.WebhookToken)
        {
            _logger.LogWarning("Unauthorized webhook attempt from IP: {IP}. Missing or invalid 'asaas-access-token' header.", HttpContext.Connection.RemoteIpAddress);
            return Unauthorized();
        }

        try
        {
            var eventData = JsonSerializer.Deserialize<AsaasWebhookEvent>(rawEvent.GetRawText(), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (eventData == null || string.IsNullOrEmpty(eventData.Event))
                return BadRequest("Invalid event data");

            // Use a unique ID for idempotency (e.g., event ID or payment ID + status)
            // Asaas typically provides an ID in the webhook headers or payload
            var eventId = Request.Headers.TryGetValue("event-id", out var id) ? id.ToString() : eventData.Payment.Id + "_" + eventData.Event;

            // 2. Idempotency Check: Prevent duplicate processing
            var alreadyProcessed = await _context.ProcessedWebhooks.AnyAsync(w => w.EventId == eventId);
            if (alreadyProcessed)
            {
                _logger.LogInformation("Webhook event {EventId} already processed. Skipping.", eventId);
                return Ok();
            }

            // 3. Process Business Logic based on Event Type
            await ProcessPaymentEvent(eventData);

            // 4. Record the event as processed
            _context.ProcessedWebhooks.Add(new ProcessedWebhook
            {
                EventId = eventId,
                PaymentId = eventData.Payment.Id,
                EventType = eventData.Event,
                RawData = rawEvent.GetRawText()
            });

            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Asaas webhook");
            return StatusCode(500);
        }
    }

    private async Task ProcessPaymentEvent(AsaasWebhookEvent eventData)
    {
        // Find the Tenant associated with the Asaas Customer ID
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.AsaasCustomerId == eventData.Payment.Customer);

        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for Asaas Customer ID: {CustomerId}", eventData.Payment.Customer);
            return;
        }

        switch (eventData.Event)
        {
            case "PAYMENT_CONFIRMED":
            case "PAYMENT_RECEIVED":
                tenant.Status = TenantStatus.Active;
                _logger.LogInformation("Tenant {TenantId} activated via payment confirmation", tenant.Id);
                
                // Trigger Payment Confirmed Email (Non-blocking)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var placeholders = new Dictionary<string, string>
                        {
                            { "CompanyName", "SaasAsaasApp" },
                            { "Amount", eventData.Payment.Value.ToString("C") },
                            { "Date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                        };
                        await _emailService.SendEmailAsync(tenant.Email, "Payment Confirmed - SaasAsaasApp", "payment-confirmed.html", placeholders);
                        _logger.LogInformation("Payment confirmation email triggered for {Email}", tenant.Email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending payment confirmation email to {Email}", tenant.Email);
                    }
                });
                break;

            case "PAYMENT_OVERDUE":
                tenant.Status = TenantStatus.Overdue;
                _logger.LogWarning("Tenant {TenantId} marked as OVERDUE", tenant.Id);

                // Trigger Payment Overdue Email (Non-blocking)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var placeholders = new Dictionary<string, string>
                        {
                            { "CompanyName", "SaasAsaasApp" },
                            { "BillingUrl", "https://your-app.com/billing" } // Should be dynamic in production
                        };
                        await _emailService.SendEmailAsync(tenant.Email, "Payment Overdue - SaasAsaasApp", "payment-overdue.html", placeholders);
                        _logger.LogInformation("Payment overdue email triggered for {Email}", tenant.Email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending payment overdue email to {Email}", tenant.Email);
                    }
                });
                break;

            case "PAYMENT_DELETED":
                // Handle payment cancellation if necessary
                break;
        }
    }
}
