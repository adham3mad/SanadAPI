using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanad.Models.Data;
using SanadAPI.DTOs;
using System.Security.Claims;

namespace SanadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DbEntity context;

        public UsersController(DbEntity _context)
        {
            context = _context;
        }

        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        private string GetCurrentUserRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "";

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var currentRole = GetCurrentUserRole();

            

            var users = await context.Users
                .Include(u => u.Conversations)
                .ThenInclude(c => c.Messages)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Image = u.Image,
                    Email = u.Email,
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
                Role = dto.Role
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

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

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);

            if (!string.IsNullOrWhiteSpace(dto.Role))
                user.Role = dto.Role;

            await context.SaveChangesAsync();
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

            foreach (var conv in user.Conversations)
                context.Messages.RemoveRange(conv.Messages);

            context.Conversations.RemoveRange(user.Conversations);
            context.Users.Remove(user);

            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
