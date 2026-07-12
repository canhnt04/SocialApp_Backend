using MediatR;
using SocialApp.PostService.Application.DTOs;

namespace SocialApp.PostService.Application.Queries;

public record GetCommentsByPostIdQuery(
    Guid PostId
) : IRequest<IEnumerable<CommentDto>>;