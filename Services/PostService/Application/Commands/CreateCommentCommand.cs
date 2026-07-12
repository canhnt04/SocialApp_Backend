using MediatR;
using SocialApp.PostService.Application.DTOs;

namespace SocialApp.PostService.Application.Commands;

public record CreateCommentCommand(
    Guid PostId,
    Guid AuthorId,
    CreateCommentDto Dto
) : IRequest<CommentDto>;