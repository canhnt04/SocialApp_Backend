using MediatR;
using SocialApp.PostService.Application.DTOs;
using SocialApp.PostService.Domain.Entities;

namespace SocialApp.PostService.Application.Commands;

public record CreatePostCommand(
    Guid AuthorId,
    string AuthorUsername,
    string Content,
    string? ImageUrl,
    string? VideoUrl,
    PostVisibility Visibility = PostVisibility.Public
) : IRequest<PostDto>;
