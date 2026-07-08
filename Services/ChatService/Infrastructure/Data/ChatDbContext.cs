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
            entity.ToTable("messages");
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
            entity.ToTable("chats");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.PrivateKey).HasMaxLength(100);
            entity.HasIndex(e => e.PrivateKey)
                  .IsUnique()
                  .HasFilter("\"Type\" = 'Private'");
        });

        modelBuilder.Entity<ChatUser>(entity =>
        {
            entity.ToTable("chat-users");
            entity.HasKey(e => new { e.ChatId, e.UserId });
            entity.HasOne(e => e.Chat)
                  .WithMany(g => g.Members)
                  .HasForeignKey(e => e.ChatId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
