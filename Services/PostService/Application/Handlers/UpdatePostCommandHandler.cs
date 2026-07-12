using MediatR;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Application.DTOs;
using SocialApp.PostService.Domain.Repositories;

namespace SocialApp.PostService.Application.Handlers;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, PostDto>
{
    private readonly IPostRepository _repository;

    public UpdatePostCommandHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<PostDto> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy bài viết từ Repo
        var post = await _repository.GetByIdAsync(request.PostId, cancellationToken);
        if (post == null)
        {
            throw new KeyNotFoundException("Bài viết không tồn tại.");
        }

        // 2. PHÂN QUYỀN: Kiểm tra xem user yêu cầu có phải tác giả không
        if (post.AuthorId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa bài viết này.");
        }

        // 3. Cập nhật thông tin chữ và chế độ hiển thị
        post.Content = request.Content;
        post.Visibility = request.Visibility;
        post.UpdatedAt = DateTime.UtcNow;

        // 4. Lưu thay đổi qua phương thức Update sẵn có trong Repository của bạn
        _repository.Update(post);
        await _repository.SaveChangesAsync(cancellationToken);

        // 5. Trả về DTO mới
        return new PostDto(
            post.Id, post.AuthorId, post.AuthorUsername, post.Content,
            null, null, post.LikeCount, post.CommentCount,
            post.ShareCount, post.IsActive, post.Visibility,
            post.CreatedAt, post.UpdatedAt
        );
    }
}