using Microsoft.EntityFrameworkCore;
using SocialApp.ChatService.Domain.Entities;

namespace SocialApp.ChatService.Infrastructure.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatUser> ChatUsers => Set<ChatUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SenderId);
            entity.HasIndex(e => e.ChatId);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SenderUsername).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.Chat)
                  .WithMany(g => g.Messages)
                  .HasForeignKey(e => e.ChatId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.ToTable("Chats");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Type).HasConversion<string>();
        });

        modelBuilder.Entity<ChatUser>(entity =>
        {
            entity.ToTable("ChatUsers");
            entity.HasKey(e => new { e.ChatId, e.UserId });
            entity.HasOne(e => e.Chat)
                  .WithMany(g => g.Members)
                  .HasForeignKey(e => e.ChatId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
