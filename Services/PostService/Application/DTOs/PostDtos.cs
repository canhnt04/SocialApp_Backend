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

public record LikePostDto(
    Guid AuthorId
);

public record LikeResponseDto(
    Guid PostId,
    bool IsLiked,
    int LikeCount
);

public record CreateCommentDto(
    Guid AuthorId,
    string Content,
    Guid? ParentId = null
);

public record CommentDto(
    Guid Id,
    Guid PostId,
    Guid AuthorId,
    Guid? ParentId,
    string Content,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
