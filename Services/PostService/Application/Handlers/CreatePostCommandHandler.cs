using MediatR;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Application.DTOs;
using SocialApp.PostService.Domain.Entities;
using SocialApp.PostService.Domain.Repositories;
using SocialApp.PostService.Infrastructure.Messaging;
using System.Text.Json;

namespace SocialApp.PostService.Application.Handlers;

public class CreatePostCommandHandler : IRequestHandler<SoftCreatePostCommand, PostDto>
{
    private readonly IPostRepository _repository;
    private readonly MessageBroker? _messageBroker;

    public CreatePostCommandHandler(IPostRepository repository, MessageBroker? messageBroker = null)
    {
        _repository = repository;
        _messageBroker = messageBroker;
    }

    public async Task<PostDto> Handle(SoftCreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post
        {
            AuthorId = request.AuthorId,
            AuthorUsername = request.AuthorUsername,
            Content = request.Content,
            Visibility = request.Visibility
        };

        await _repository.AddAsync(post, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Phát sự kiện qua RabbitMQ
        try
        {
            _messageBroker?.Publish("post.created", JsonSerializer.Serialize(new
            {
                PostId = post.Id,
                post.AuthorId,
                post.AuthorUsername,
                post.CreatedAt
            }));
        }
        catch { }

        return new PostDto(
            post.Id, post.AuthorId, post.AuthorUsername, post.Content,
            null, null, post.LikeCount, post.CommentCount,
            post.ShareCount, post.IsActive, post.Visibility,
            post.CreatedAt, post.UpdatedAt
        );
    }
}
