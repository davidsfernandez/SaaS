using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaasAsaasApp.Data.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string templateName, Dictionary<string, string> placeholders);
}
