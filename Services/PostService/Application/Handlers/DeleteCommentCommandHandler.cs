using MediatR;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Domain.Repositories;

namespace SocialApp.PostService.Application.Handlers;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly IPostRepository _repository;

    public DeleteCommentCommandHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repository.GetCommentByIdAsync(request.CommentId, cancellationToken);
        if (comment == null)
        {
            throw new KeyNotFoundException("Bình luận không tồn tại.");
        }

        // XÉT AUTHORIZE: Chỉ chính chủ nhân của comment mới được xóa
        if (comment.AuthorId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xóa bình luận này.");
        }

        // Thay đổi trạng thái xóa mềm
        comment.IsActive = false;
        comment.Content = "Bình luận này đã bị xóa.";
        comment.UpdatedAt = DateTime.UtcNow;

        // Tái sử dụng cùng một hàm UpdateComment
        _repository.UpdateComment(comment);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}