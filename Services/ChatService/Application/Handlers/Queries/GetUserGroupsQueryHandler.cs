using MediatR;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Application.Queries;
using SocialApp.ChatService.Domain.Repositories;

namespace SocialApp.ChatService.Application.Handlers.Queries;

public class GetUserGroupsQueryHandler : IRequestHandler<GetUserGroupsQuery, IEnumerable<GroupDto>>
{
    private readonly IChatRepository _repository;

    public GetUserGroupsQueryHandler(IChatRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<GroupDto>> Handle(GetUserGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _repository.GetUserGroupsAsync(request.UserId, cancellationToken);
        
        return groups.Select(g => new GroupDto(
            g.Id,
            g.Name ?? string.Empty,
            g.CreatorId ?? Guid.Empty,
            g.Members.Select(m => m.UserId).ToList(),
            g.CreatedAt,
            g.Members.Count
        ));
    }
}
