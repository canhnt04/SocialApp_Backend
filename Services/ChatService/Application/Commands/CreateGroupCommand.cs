using MediatR;
using SocialApp.ChatService.Application.DTOs;

namespace SocialApp.ChatService.Application.Commands;

public record CreateGroupCommand(
    string Name,
    string? Description,
    Guid CreatedByUserId,
    List<GroupMemberDto> Members
) : IRequest<GroupDto>;
