using MediatR;
using SocialApp.PostService.Application.DTOs;

namespace SocialApp.PostService.Application.Commands;

public record SharePostCommand(
    Guid TargetPostId, // ID của bài viết muốn chia sẻ
    Guid AuthorId,     // ID của người bấm nút chia sẻ
    string AuthorUsername,
    string? Content    // Nội dung/cảm nghĩ đi kèm khi share (có thể để trống)
) : IRequest<PostDto>;