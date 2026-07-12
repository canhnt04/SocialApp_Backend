using MediatR;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Application.DTOs;
using SocialApp.PostService.Domain.Entities;
using SocialApp.PostService.Domain.Repositories;
using SocialApp.PostService.Infrastructure.Messaging;
using System.Text.Json;

namespace SocialApp.PostService.Application.Handlers;

public class LikePostCommandHandler : IRequestHandler<LikePostCommand, LikeResponseDto>
{
    private readonly IPostRepository _repository;
    private readonly MessageBroker? _messageBroker;

    public LikePostCommandHandler(IPostRepository repository, MessageBroker? messageBroker = null)
    {
        _repository = repository;
        _messageBroker = messageBroker;
    }

    public async Task<LikeResponseDto> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetByIdAsync(request.PostId, cancellationToken);
        if (post == null)
        {
            throw new KeyNotFoundException("Không tìm thấy bài viết");
        }

        var like = await _repository.GetLikeAsync(request.AuthorId, request.PostId, cancellationToken);
        bool isLiked;

        if (like == null)
        {
            like = new Like
            {
                AuthorId = request.AuthorId,
                PostId = request.PostId,
                IsActive = true
            };
            await _repository.AddLikeAsync(like, cancellationToken);
            post.LikeCount++;
            isLiked = true;
        }
        else
        {
            like.IsActive = !like.IsActive;
            like.UpdatedAt = DateTime.UtcNow;
            _repository.UpdateLike(like);

            if (like.IsActive)
            {
                post.LikeCount++;
            }
            else
            {
                post.LikeCount = Math.Max(0, post.LikeCount - 1);
            }
            isLiked = like.IsActive;
        }

        _repository.Update(post);
        await _repository.SaveChangesAsync(cancellationToken);

        // Phát sự kiện qua RabbitMQ
        try
        {
            _messageBroker?.Publish("post.liked", JsonSerializer.Serialize(new
            {
                PostId = post.Id,
                AuthorId = request.AuthorId,
                IsLiked = isLiked,
                LikeCount = post.LikeCount,
                Timestamp = DateTime.UtcNow
            }));
        }
        catch { }

        return new LikeResponseDto(post.Id, isLiked, post.LikeCount);
    }
}
