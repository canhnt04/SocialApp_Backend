using MediatR;

namespace SocialApp.PostService.Application.Commands;

public record SoftDeletePostCommand(
    Guid PostId,
    Guid UserId
) : IRequest<bool>;