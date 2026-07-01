using Microsoft.EntityFrameworkCore;
using SocialApp.PostService.Domain.Entities;

namespace SocialApp.PostService.Infrastructure.Data;

public class PostDbContext : DbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Like> Likes => Set<Like>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.AuthorUsername).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Visibility)
                  .HasConversion<int>()
                  .HasDefaultValue(PostVisibility.Public);
        });

        modelBuilder.Entity<PostMedia>(entity =>
        {
            entity.ToTable("PostMedia");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MediaUrl).IsRequired();
            entity.Property(e => e.MediaType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.Property(e => e.FileSize).HasMaxLength(255).IsRequired();
            entity.HasOne(e => e.Post)
                  .WithMany(p => p.Media)
                  .HasForeignKey(e => e.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comments");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Post)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(e => e.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Parent)
                  .WithMany(c => c.Replies)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.ToTable("Likes");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AuthorId, e.PostId }).IsUnique();
            entity.HasIndex(e => new { e.AuthorId, e.CommentId }).IsUnique();
            entity.HasOne(e => e.Post)
                  .WithMany(p => p.Likes)
                  .HasForeignKey(e => e.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Comment)
                  .WithMany(c => c.Likes)
                  .HasForeignKey(e => e.CommentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
