using SocialApp.PostService.Domain.Entities;

namespace SocialApp.PostService.Application.DTOs;

public record CreatePostDto(
    Guid AuthorId,
    string AuthorUsername,
    string Content,
    string? ImageUrl,
    string? VideoUrl,
    PostVisibility Visibility = PostVisibility.Public
);

public record UpdatePostDto(
    string? Content,
    string? ImageUrl,
    string? VideoUrl,
    PostVisibility? Visibility
);

public record PostDto(
    Guid Id,
    Guid AuthorId,
    string AuthorUsername,
    string Content,
    string? ImageUrl,
    string? VideoUrl,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    bool IsPublished,
    PostVisibility Visibility,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
