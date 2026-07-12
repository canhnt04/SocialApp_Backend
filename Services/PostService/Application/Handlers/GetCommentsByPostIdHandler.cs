using MediatR;
using SocialApp.PostService.Application.DTOs;
using SocialApp.PostService.Application.Queries;
using SocialApp.PostService.Domain.Repositories;

namespace SocialApp.PostService.Application.Handlers;

public class GetCommentsByPostIdQueryHandler : IRequestHandler<GetCommentsByPostIdQuery, IEnumerable<CommentDto>>
{
    private readonly IPostRepository _repository;

    public GetCommentsByPostIdQueryHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CommentDto>> Handle(GetCommentsByPostIdQuery request, CancellationToken cancellationToken)
    {
        var comments = await _repository.GetCommentsByPostIdAsync(request.PostId, cancellationToken);

        return comments.Select(c => new CommentDto(
            c.Id,
            c.PostId,
            c.AuthorId,
            c.ParentId,
            c.Content,
            c.IsActive,
            c.CreatedAt,
            c.UpdatedAt
        ));
    }
}