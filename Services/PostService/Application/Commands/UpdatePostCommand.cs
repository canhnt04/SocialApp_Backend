using MediatR;
using SocialApp.PostService.Application.DTOs;
using SocialApp.PostService.Domain.Entities;

namespace SocialApp.PostService.Application.Commands;

public record UpdatePostCommand(
    Guid PostId,
    Guid UserId,
    string Content,
    PostVisibility Visibility
) : IRequest<PostDto>;