[HttpGet("test-email")]
public async Task<IActionResult> TestEmail()
{
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress("Sanad App", "your@gmail.com"));
    message.To.Add(new MailboxAddress("Test", "test@example.com"));
    message.Subject = "Test Email";
    message.Body = new TextPart("plain") { Text = "Hello from Railway" };

    using var client = new SmtpClient();
    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
    await client.AuthenticateAsync("your@gmail.com", "your_app_password");
    await client.SendAsync(message);
    await client.DisconnectAsync(true);

    return Ok("Email sent!");
}
