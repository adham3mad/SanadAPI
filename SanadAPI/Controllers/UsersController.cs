using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sanad.DTOs;
using Sanad.Models.Data;
using SanadAPI.DTOs;
using System.Security.Cryptography;

namespace SanadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DbEntity context;
        private readonly IEmailService emailService;
        private static Dictionary<Guid, (string Token, DateTime Expiry)> _verificationTokens = new();

        public UsersController(DbEntity _context, IEmailService _emailService)
        {
            context = _context;
            emailService = _emailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await context.Users
                .Include(u => u.Conversations)
                .ThenInclude(c => c.Messages)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Image = u.Image,
                    Email = u.Email,
                    Role = u.Role,
                    Conversations = u.Conversations.Select(c => new ConversationDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        CreatedAt = c.CreatedAt,
                        Messages = c.Messages.Select(m => new MessageDto
                        {
                            Id = m.Id,
                            Role = m.Role,
                            Content = m.Content,
                            CreatedAt = m.CreatedAt
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await context.Users
                .Include(u => u.Conversations)
                .ThenInclude(c => c.Messages)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Image = user.Image,
                Email = user.Email,
                Role = user.Role,
                Conversations = user.Conversations.Select(c => new ConversationDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    CreatedAt = c.CreatedAt,
                    Messages = c.Messages.Select(m => new MessageDto
                    {
                        Id = m.Id,
                        Role = m.Role,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt
                    }).ToList()
                }).ToList()
            };
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Image = dto.Image,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash),
                Role = dto.Role,
                IsEmailConfirmed = false
            };

            context.Users.Add(user);
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

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            _verificationTokens[user.Id] = (token, DateTime.UtcNow.AddHours(24));
            var verificationLink = $"https://your-backend-domain.com/api/auth/verify-email?userId={user.Id}&token={token}";

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

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Image = user.Image,
                Email = user.Email,
                Role = user.Role,
                Conversations = new List<ConversationDto>()
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Image))
                user.Image = dto.Image;

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                user.Email = dto.Email;
                user.IsEmailConfirmed = false;

                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                _verificationTokens[user.Id] = (token, DateTime.UtcNow.AddHours(24));
                var verificationLink = $"https://your-backend-domain.com/api/auth/verify-email?userId={user.Id}&token={token}";

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
            }

            if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);

            if (!string.IsNullOrWhiteSpace(dto.Role))
                user.Role = dto.Role;

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

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await context.Users
                .Include(u => u.Conversations)
                .ThenInclude(c => c.Messages)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            if (user.Conversations != null)
                context.Messages.RemoveRange(user.Conversations.SelectMany(c => c.Messages));

            if (user.Conversations != null)
                context.Conversations.RemoveRange(user.Conversations);

            context.Users.Remove(user);

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


            return NoContent();
        }
    }
}
