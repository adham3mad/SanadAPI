using System.ComponentModel.DataAnnotations;

namespace SanadAPI.DTOs
{
    public class CreateConversationDto
    {
        public string Title { get; set; }
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }
    }

    public class ConversationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MessageDto> Messages { get; set; }
    }
}
