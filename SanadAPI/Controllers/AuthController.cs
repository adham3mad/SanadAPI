using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sanad.DTOs;
using Sanad.Models.Data;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sanad.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DbEntity context;
        private readonly IConfiguration config;
<<<<<<< HEAD
        private readonly IEmailService emailService;
        private static Dictionary<Guid, (string Token, DateTime Expiry)> verificationTokens = new();

        public AuthController(DbEntity _context, IConfiguration _config, IEmailService _emailService)
        {
            context = _context;
            config = _config;
            emailService = _emailService;
=======
        private readonly EmailSettings emailSettings;
        private static Dictionary<Guid, (string Token, DateTime Expiry)> _verificationTokens = new();

        public AuthController(DbEntity _context, IConfiguration _config, IOptions<EmailSettings> _emailSettings)
        {
            context = _context;
            config = _config;
            emailSettings = _emailSettings.Value;
>>>>>>> parent of b993692 (add email verification for creat user and update user)
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest("Email already exists");

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                IsEmailConfirmed = false
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
<<<<<<< HEAD
            verificationTokens[user.Id] = (token, DateTime.UtcNow.AddHours(24));
            var verificationLink = $"https://adham3mad.github.io/Confirm-Email-Address/?userId={user.Id}&token={token}";

            try
            {
                await emailService.SendEmailAsync(
                    user.Email,
                    user.Name,
                    "Verify your email",
                    $@"
                    <h2>Welcome {user.Name}</h2>
                    <p>Please verify your email by clicking the link below:</p>
                    <p><a href='{verificationLink}' target='_blank'>Verify Email</a></p>
                    <br/>
                    <p>This link will expire in 24 hours.</p>"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
=======
            var expiry = DateTime.UtcNow.AddHours(24);
            _verificationTokens[user.Id] = (token, expiry);

            var verificationLink = $"https://adham3mad.github.io/Confirm-Email-Address/?userId={user.Id}&token={token}";

            await SendEmailAsync(
                user.Email,
                user.Name,
                "Verify your email",
                $@"
                <h2>Welcome {user.Name}</h2>
                <p>Please verify your email by clicking the link below:</p>
                <p><a href='{verificationLink}' target='_blank'>Verify Email</a></p>
                <br/>
                <p>This link will expire in 24 hours.</p>"
            );
>>>>>>> parent of b993692 (add email verification for creat user and update user)

            return Ok(new { message = "User registered successfully. Please check your email to verify your account." });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(Guid userId, string token)
        {
            if (!_verificationTokens.ContainsKey(userId))
                return BadRequest("Invalid or expired token");

            var (savedToken, expiry) = _verificationTokens[userId];
            if (savedToken != token || expiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired token");

            var user = await context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.IsEmailConfirmed = true;
<<<<<<< HEAD
<<<<<<< HEAD
            await context.SaveChangesAsync();
=======
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveChangesAsync failed:");
                Console.WriteLine(ex.ToString()); // هيوضح الاستثناء بالكامل
                return StatusCode(500, ex.Message);
            }

>>>>>>> parent of c3553dd (Update AuthController.cs)
=======
            await context.SaveChangesAsync();
>>>>>>> parent of c618bc1 (edit)

<<<<<<< HEAD
            _verificationTokens.Remove(userId);
=======
            verificationTokens.Remove(userId);
>>>>>>> parent of c618bc1 (edit)

            return Ok("Email verified successfully! You can now log in.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            try
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                    return Unauthorized("Invalid credentials");

                if (!user.IsEmailConfirmed)
                    return Unauthorized("Please verify your email before logging in.");

                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Login failed: {ex.Message}");
            }
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDTO dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return NotFound("Email not found");

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
<<<<<<< HEAD
            verificationTokens[user.Id] = (token, DateTime.UtcNow.AddMinutes(15));

            var resetLink = $"https://adham3mad.github.io/Reset-Password-Sanad?userId={user.Id}&token={token}";

            try
            {
                await emailService.SendEmailAsync(
                    user.Email,
                    user.Name,
                    "Password Reset Request",
                    $@"
                    <h2>Password Reset Request</h2>
                    <p>We received a request to reset your password.</p>
                    <p>Click the link below to reset your password:</p>
                    <p><a href='{resetLink}' target='_blank'>Reset Password</a></p>
                    <br/>
                    <p><b>Note:</b> This link will expire in 15 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
=======
            var expiry = DateTime.UtcNow.AddMinutes(15);
            _verificationTokens[user.Id] = (token, expiry);

            var resetLink = $"https://adham3mad.github.io/Reset-Password-Sanad?userId={user.Id}&token={token}";

            await SendEmailAsync(
                user.Email,
                user.Name,
                "Password Reset Request",
                $@"
                <h2>Password Reset Request</h2>
                <p>We received a request to reset your password.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetLink}' target='_blank'>Reset Password</a></p>
                <br/>
                <p><b>Note:</b> This link will expire in 15 minutes.</p>
                <p>If you didn't request this, please ignore this email.</p>"
            );
>>>>>>> parent of b993692 (add email verification for creat user and update user)

            return Ok("Password reset link has been sent to your email (valid 15 minutes)");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            if (!_verificationTokens.TryGetValue(dto.UserId, out var tokenData))
                return BadRequest("Invalid or expired token");

            var (storedToken, expiry) = tokenData;
            if (storedToken != dto.Token || DateTime.UtcNow > expiry)
                return BadRequest("Invalid or expired token");

            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null) return NotFound("User not found");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
<<<<<<< HEAD
            _verificationTokens.Remove(dto.UserId);
=======
            verificationTokens.Remove(dto.UserId);
>>>>>>> parent of c618bc1 (edit)

            await context.SaveChangesAsync();
            return Ok("Password has been reset successfully");
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent)
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
}
