using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Sanad.Models.Data
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
        public string Name { get; set; }
        public string? Image { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User";
        public bool IsEmailConfirmed { get; set; } = false;
        public virtual List<Conversation>? Conversations { get; set; }
    }
}
