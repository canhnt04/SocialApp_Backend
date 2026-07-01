using Microsoft.EntityFrameworkCore;
using SocialApp.PostService.Domain.Entities;

namespace SocialApp.PostService.Infrastructure.Data;

public class PostDbContext : DbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { }

    public DbSet<Post> Posts => Set<Post>();

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
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.VideoUrl).HasMaxLength(1000);
            entity.Property(e => e.Visibility)
                  .HasConversion<int>()
                  .HasDefaultValue(PostVisibility.Public);
        });
    }
}
