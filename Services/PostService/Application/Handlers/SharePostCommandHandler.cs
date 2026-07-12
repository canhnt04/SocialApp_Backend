using MediatR;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Application.DTOs;
using SocialApp.PostService.Domain.Entities;
using SocialApp.PostService.Domain.Repositories;

namespace SocialApp.PostService.Application.Handlers;

public class SharePostCommandHandler : IRequestHandler<SharePostCommand, PostDto>
{
    private readonly IPostRepository _repository;

    public SharePostCommandHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<PostDto> Handle(SharePostCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra bài viết gốc có tồn tại không
        var originalPost = await _repository.GetByIdAsync(request.TargetPostId, cancellationToken);
        if (originalPost == null)
        {
            throw new KeyNotFoundException("Bài viết gốc không tồn tại.");
        }

        // 2. Tăng số lượt share của bài viết gốc lên 1
        originalPost.ShareCount += 1;
        _repository.Update(originalPost);

        // 3. Tạo bài viết mới dạng "Share"
        var sharedPost = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = request.AuthorId,
            AuthorUsername = request.AuthorUsername,
            Content = request.Content ?? string.Empty,
            OriginalPostId = originalPost.Id, // Khớp với trường OriginalPostId sẵn có của bạn
            Visibility = PostVisibility.Public,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(sharedPost, cancellationToken);

        // 4. Lưu tất cả thay đổi vào DB
        await _repository.SaveChangesAsync(cancellationToken);

        // 5. Trả về thông tin bài viết mới tạo
        return new PostDto(
            sharedPost.Id,
            sharedPost.AuthorId,
            sharedPost.AuthorUsername,
            sharedPost.Content,
            null, // Bài share thường không copy file vật lý mà hiển thị gián tiếp từ bài gốc
            null,
            0,
            0,
            0,
            true,
            sharedPost.Visibility,
            sharedPost.CreatedAt,
            sharedPost.UpdatedAt
        );
    }
}