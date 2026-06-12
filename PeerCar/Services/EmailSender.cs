using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CarRentalMVC.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For development, just log the email content
            _logger.LogInformation($"Sending email to {email}: {subject}\n{htmlMessage}");
            return Task.CompletedTask;
        }
    }
}
