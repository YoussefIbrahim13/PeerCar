using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace CarRentalMVC.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly string _apiKey;
        private readonly ILogger<SendGridEmailSender> _logger;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public SendGridEmailSender(IConfiguration config, ILogger<SendGridEmailSender> logger)
        {
            _apiKey = config["SendGrid:ApiKey"] ?? string.Empty;
            _logger = logger;
            _senderEmail = config["SendGrid:SenderEmail"] ?? "noreply@peercar.com";
            _senderName = config["SendGrid:SenderName"] ?? "PeerCar";
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("SendGrid API key is missing. Email not sent to {Email}.", email);
                return;
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogError("Recipient email is null or empty. Email not sent.");
                return;
            }
            try
            {
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(_senderEmail, _senderName);
                var to = new EmailAddress(email);
                var msg = MailHelper.CreateSingleEmail(from, to, subject ?? string.Empty, null, htmlMessage ?? string.Empty);
                var response = await client.SendEmailAsync(msg);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent to {Email} via SendGrid. Status: {StatusCode}", email, response.StatusCode);
                }
                else
                {
                    var body = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send email to {Email} via SendGrid. Status: {StatusCode}. Body: {Body}", email, response.StatusCode, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email to {Email} via SendGrid.", email);
            }
        }
    }
}
