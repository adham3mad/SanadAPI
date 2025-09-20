using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sanad.DTOs;
using SendGrid;
using SendGrid.Helpers.Mail;

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
                var client = new SendGridClient(_emailSettings.ApiKey);

                var from = new EmailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                var subject = "Test Email from Sanad";
                var to = new EmailAddress("test@example.com", "Test User");
                var plainTextContent = "Hello from Railway using SendGrid!";
                var htmlContent = "<strong>Hello from Railway using SendGrid!</strong>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                    return Ok("Email sent successfully via SendGrid!");
                else
                    return StatusCode((int)response.StatusCode, "SendGrid failed to send email.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Email sending failed: {ex.Message}");
            }
        }
    }
}
