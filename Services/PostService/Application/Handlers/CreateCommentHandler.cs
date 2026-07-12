using MediatR;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Application.DTOs;
using SocialApp.PostService.Domain.Entities;
using SocialApp.PostService.Domain.Repositories;

namespace SocialApp.PostService.Application.Handlers;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IPostRepository _repository;

    public CreateCommentCommandHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra bài viết tồn tại
        var post = await _repository.GetByIdAsync(request.PostId, cancellationToken);
        if (post == null)
        {
            throw new KeyNotFoundException("Bài viết không tồn tại.");
        }

        // 2. Tạo Entity Comment mới
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = request.PostId,
            AuthorId = request.AuthorId,
            ParentId = request.Dto.ParentId, // Hỗ trợ reply comment nếu ParentId khác null
            Content = request.Dto.Content,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 3. Lưu vào database
        await _repository.AddCommentAsync(comment, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // TODO: Phát event CommentAdded sang RabbitMQ tại đây cho NotificationService[cite: 1]

        // 4. Mapping sang DTO trả về cho client
        return new CommentDto(
            comment.Id,
            comment.PostId,
            comment.AuthorId,
            comment.ParentId,
            comment.Content,
            comment.IsActive,
            comment.CreatedAt,
            comment.UpdatedAt
        );
    }
}