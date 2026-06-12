using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var settings = _config.GetSection("EmailSettings");
        var mail = settings["SenderEmail"];
        var pw = settings["Password"];

        using var client = new SmtpClient(settings["SmtpServer"], int.Parse(settings["Port"]!))
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(mail, pw)
        };

        var message = new MailMessage(mail!, email, subject, htmlMessage)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message);
    }
}