using MediatR;
using SocialApp.ChatService.Application.DTOs;

namespace SocialApp.ChatService.Application.Commands;

public record CreateGroupCommand(
    string Name,
    List<GroupMemberDto> Members
) : IRequest<GroupDto>;
