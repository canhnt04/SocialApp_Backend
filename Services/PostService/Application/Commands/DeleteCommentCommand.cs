using MediatR;

public record DeleteCommentCommand(
    Guid CommentId,
    Guid UserId
) : IRequest<bool>;