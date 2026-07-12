using MediatR;
using SocialApp.PostService.Application.DTOs;

public record UpdateCommentCommand(
    Guid CommentId,
    Guid UserId,
    string NewContent
) : IRequest<CommentDto>;