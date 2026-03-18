using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaasAsaasApp.Configurations;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Services;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly SendGridSettings _settings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        HttpClient httpClient,
        IOptions<SendGridSettings> settings,
        IWebHostEnvironment environment,
        ILogger<EmailService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string templateName, Dictionary<string, string> placeholders)
    {
        try
        {
            var htmlContent = await GetTemplateContentAsync(templateName, placeholders);

            var payload = new
            {
                personalizations = new[]
                {
                    new
                    {
                        to = new[] { new { email = toEmail } },
                        subject = subject
                    }
                },
                from = new { email = _settings.FromEmail },
                content = new[]
                {
                    new
                    {
                        type = "text/html",
                        value = htmlContent
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var response = await _httpClient.PostAsync("https://api.sendgrid.com/v3/mail/send", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {ToEmail} with subject '{Subject}'", toEmail, subject);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {ToEmail}. Status: {StatusCode}, Error: {Error}", toEmail, response.StatusCode, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {ToEmail}", toEmail);
        }
    }

    private async Task<string> GetTemplateContentAsync(string templateName, Dictionary<string, string> placeholders)
    {
        var templatePath = Path.Combine(_environment.WebRootPath, "templates", "email", $"{templateName}.html");

        if (!File.Exists(templatePath))
        {
            _logger.LogError("Email template not found: {TemplatePath}", templatePath);
            throw new FileNotFoundException($"Email template not found: {templatePath}");
        }

        var content = await File.ReadAllTextAsync(templatePath);

        foreach (var placeholder in placeholders)
        {
            content = content.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        }

        return content;
    }
}
