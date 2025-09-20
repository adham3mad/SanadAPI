using Microsoft.Extensions.Options;
using Sanad.DTOs;
using SendGrid;
using SendGrid.Helpers.Mail;

public class SendGridEmailService : IEmailService
{
    private readonly EmailSettings emailSettings;

    public SendGridEmailService(IOptions<EmailSettings> options)
    {
        emailSettings = options.Value;
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent)
    {
        var client = new SendGridClient(emailSettings.ApiKey);

        var from = new EmailAddress(emailSettings.SenderEmail, emailSettings.SenderName);

        var to = new EmailAddress(toEmail, toName);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: "", htmlContent: htmlContent);

        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync();
            throw new Exception($"SendGrid failed: {response.StatusCode}, {body}");
        }
    }
}
