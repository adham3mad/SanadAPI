using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanad.Models.Data;
using SanadAPI.DTOs;
using SanadAPI.Models.Data;
using System.Security.Claims;

namespace SanadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly DbEntity context;

        public MessagesController(DbEntity _context)
        {
            context = _context;
        }

        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private string GetCurrentUserRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "";

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var conv = await context.Conversations.FindAsync(dto.ConversationId);
            if (conv == null) return NotFound();

            if (currentRole != "Admin" && conv.User_Id != currentUserId)
                return Forbid();

            var message = new Message
            {
                Role = dto.Role,
                Content = dto.Content,
                Conversation_Id = dto.ConversationId,
                CreatedAt = DateTime.Now
            };

            context.Messages.Add(message);
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


            return new MessageDto
            {
                Id = message.Id,
                Role = message.Role,
                Content = message.Content,
                CreatedAt = message.CreatedAt
            };
        }

        [HttpGet("conversation/{conversationId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetConversationMessages(int conversationId)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var conv = await context.Conversations.FindAsync(conversationId);
            if (conv == null) return NotFound();

            if (currentRole != "Admin" && conv.User_Id != currentUserId)
                return Forbid();

            var messages = await context.Messages
                .Where(m => m.Conversation_Id == conversationId)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            if (!messages.Any())
                return NotFound($"No messages found for conversation {conversationId}");

            return messages;
        }
    }
}
