using Sanad.Models.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanadAPI.Models.Data
{
    public class Message
    {
        public int Id { get; set; }
        public string? Role { get; set; } 
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("Conversation")]
        public int Conversation_Id { get; set; }
        public virtual Conversation Conversation { get; set; }
    }
}
