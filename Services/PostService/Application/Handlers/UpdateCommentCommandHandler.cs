using MediatR;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Application.DTOs;
using SocialApp.PostService.Domain.Repositories;

namespace SocialApp.PostService.Application.Handlers;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private readonly IPostRepository _repository;

    public UpdateCommentCommandHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repository.GetCommentByIdAsync(request.CommentId, cancellationToken);
        if (comment == null)
        {
            throw new KeyNotFoundException("Bình luận không tồn tại.");
        }

        // XÉT AUTHORIZE: Chỉ chính chủ nhân của comment mới được sửa
        if (comment.AuthorId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa bình luận này.");
        }

        // Cập nhật các thông tin thay đổi
        comment.Content = request.NewContent;
        comment.UpdatedAt = DateTime.UtcNow;

        // Tái sử dụng hàm UpdateComment
        _repository.UpdateComment(comment);
        await _repository.SaveChangesAsync(cancellationToken);

        return new CommentDto(
            comment.Id, comment.PostId, comment.AuthorId,
            comment.ParentId, comment.Content, comment.IsActive,
            comment.CreatedAt, comment.UpdatedAt
        );
    }
}