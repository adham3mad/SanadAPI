using Microsoft.EntityFrameworkCore;
using SanadAPI.Models.Data;

namespace Sanad.Models.Data
{
    public class DbEntity : DbContext
    {
        public DbEntity(DbContextOptions<DbEntity> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Conversations)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.User_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Conversation>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.Conversation_Id)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("User");
        }
    }
}
