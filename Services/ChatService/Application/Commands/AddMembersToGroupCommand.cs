using MediatR;
using SocialApp.ChatService.Application.DTOs;

namespace SocialApp.ChatService.Application.Commands;

public record AddMembersToGroupCommand(
    Guid GroupId,
    List<Guid> Members
) : IRequest<GroupDto>;
