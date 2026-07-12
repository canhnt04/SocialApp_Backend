using MediatR;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Domain.Repositories;

namespace SocialApp.PostService.Application.Handlers;

public class SoftDeletePostCommandHandler : IRequestHandler<SoftDeletePostCommand, bool>
{
    private readonly IPostRepository _repository;

    public SoftDeletePostCommandHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(SoftDeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetByIdAsync(request.PostId, cancellationToken);
        if (post == null)
        {
            throw new KeyNotFoundException("Bài viết không tồn tại.");
        }

        // PHÂN QUYỀN: Chỉ chủ bài viết mới được xóa mềm
        if (post.AuthorId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xóa bài viết này.");
        }

        // SOFT DELETE: Chuyển trạng thái hoạt động thành false
        post.IsActive = false;
        post.UpdatedAt = DateTime.UtcNow;

        _repository.Update(post); // Tái sử dụng hàm Update để cập nhật trạng thái IsActive xuống DB
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}