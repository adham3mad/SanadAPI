using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Sanad.DTOs; 

namespace Sanad.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestemailController : ControllerBase
    {
        private readonly EmailSettings _emailSettings;

        public TestemailController(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sanad App", emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("Test", "test@example.com"));
                message.Subject = "Test Email";
                message.Body = new TextPart("plain") { Text = "Hello from Railway" };

                using var client = new SmtpClient();
                await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return Ok("Email sent!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Email sending failed: {ex.Message}");
            }
        }

    }
}
