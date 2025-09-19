using System.Net;
using System.Net.Mail;

namespace SanadAPI.Controllers
{
    public class EmailController
    {
        private readonly string smtpHost;
        private readonly int smtpPort;
        private readonly string smtpUser;
        private readonly string smtpPass;
        private readonly string fromEmail;

        public EmailController()
        {
            smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "";
            smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
            smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
            smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? "";
            fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") ?? "";
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;

                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Sanad App"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mail.To.Add(to);

                await client.SendMailAsync(mail);
            }
        }
    }
}
