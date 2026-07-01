using SocialApp.Shared.Base;

namespace SocialApp.PostService.Domain.Entities;

public class PostMedia : BaseEntity
{
    public Guid PostId { get; set; }
    public string MediaUrl { get; set; } = default!;
    public string MediaType { get; set; } = default!;
    public string? MimeType { get; set; }
    public string FileSize { get; set; } = default!;
    
    public Post Post { get; set; } = default!;
}
