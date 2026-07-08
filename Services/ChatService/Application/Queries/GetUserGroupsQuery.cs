using MediatR;
using SocialApp.ChatService.Application.DTOs;

namespace SocialApp.ChatService.Application.Queries;

public record GetUserGroupsQuery(
    Guid UserId
) : IRequest<IEnumerable<GroupDto>>;
