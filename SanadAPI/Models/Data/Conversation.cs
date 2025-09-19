using SanadAPI.Models.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sanad.Models.Data
{
    public class Conversation
    {
        public int Id { get; set; }
        public String Title { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("User")]
        [Required]
        public Guid User_Id { get; set; }
        public virtual User User { get; set; }
        public List<Message> Messages { get; set; }
    }
}
