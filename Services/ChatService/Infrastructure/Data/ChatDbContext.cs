using Microsoft.EntityFrameworkCore;
using SocialApp.ChatService.Domain.Entities;

namespace SocialApp.ChatService.Infrastructure.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ChatGroup> ChatGroups => Set<ChatGroup>();
    public DbSet<ChatGroupMember> ChatGroupMembers => Set<ChatGroupMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SenderId);
            entity.HasIndex(e => e.RecipientId);
            entity.HasIndex(e => e.ChatGroupId);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SenderUsername).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.ChatGroup)
                  .WithMany(g => g.Messages)
                  .HasForeignKey(e => e.ChatGroupId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatGroup>(entity =>
        {
            entity.ToTable("ChatGroups");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<ChatGroupMember>(entity =>
        {
            entity.ToTable("ChatGroupMembers");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ChatGroupId, e.UserId }).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.ChatGroup)
                  .WithMany(g => g.Members)
                  .HasForeignKey(e => e.ChatGroupId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
