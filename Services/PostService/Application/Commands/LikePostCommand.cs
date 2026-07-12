using MediatR;
using SocialApp.PostService.Application.DTOs;

namespace SocialApp.PostService.Application.Commands;

public record LikePostCommand(
    Guid PostId,
    Guid AuthorId
) : IRequest<LikeResponseDto>;
