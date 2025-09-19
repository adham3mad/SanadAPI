using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Sanad.DTOs
{
    public class RegisterDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required]
        [StrongPassword(ErrorMessage = "Password must have at least 1 uppercase, 1 lowercase, 1 number, and 1 special character.")]
        public string Password { get; set; }

        public string Role { get; set; } = "User";
    }

    public class LoginDTO
    {
        [Required(ErrorMessage = "Email address is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }

    public class ForgetPasswordDTO
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordDTO
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        [StrongPassword(ErrorMessage = "Password must have at least 1 uppercase, 1 lowercase, 1 number, and 1 special character.")]
        public string NewPassword { get; set; }
    }


    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var password = value as string;
            if (string.IsNullOrEmpty(password)) return false;

            var hasUpper = new Regex(@"[A-Z]+");
            var hasLower = new Regex(@"[a-z]+");
            var hasDigit = new Regex(@"\d+");
            var hasSpecial = new Regex(@"[\W_]+");

            return hasUpper.IsMatch(password) && hasLower.IsMatch(password) &&
                   hasDigit.IsMatch(password) && hasSpecial.IsMatch(password);
        }
    }

    public class AllowedEmailDomainAttribute : ValidationAttribute
    {
        private readonly string[] _allowedDomains;
        public AllowedEmailDomainAttribute(string[] allowedDomains)
        {
            _allowedDomains = allowedDomains;
        }

        public override bool IsValid(object value)
        {
            var email = value as string;
            if (string.IsNullOrEmpty(email)) return false;

            try
            {
                var domain = new System.Net.Mail.MailAddress(email).Host.ToLower();
                return _allowedDomains.Contains(domain);
            }
            catch
            {
                return false;
            }
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

