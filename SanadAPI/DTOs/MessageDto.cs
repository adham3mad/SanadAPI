using System.ComponentModel.DataAnnotations;

namespace SanadAPI.DTOs
{
    public class CreateMessageDto
    {
        public string Role { get; set; }
        public string Content { get; set; }
        [Required(ErrorMessage = "Conversation ID is required")]
        public int ConversationId { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
