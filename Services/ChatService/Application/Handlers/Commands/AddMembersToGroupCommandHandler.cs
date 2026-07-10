using MediatR;
using SocialApp.ChatService.Application.Commands;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Domain.Entities;
using SocialApp.ChatService.Domain.Repositories;

namespace SocialApp.ChatService.Application.Handlers.Commands;

public class AddMembersToGroupCommandHandler : IRequestHandler<AddMembersToGroupCommand, GroupDto>
{
    private readonly IChatRepository _repository;

    public AddMembersToGroupCommandHandler(IChatRepository repository)
    {
        _repository = repository;
    }

    public async Task<GroupDto> Handle(AddMembersToGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _repository.GetGroupByIdWithMembersAsync(request.GroupId, cancellationToken);
        if (group == null)
        {
            throw new InvalidOperationException($"Group with ID {request.GroupId} not found.");
        }

        // Thêm các thành viên mới vào nhóm
        foreach (var memberId in request.Members)
        {
            var isAlreadyMember = await _repository.IsUserInChatAsync(request.GroupId, memberId, cancellationToken);
            if (!isAlreadyMember)
            {
                var groupMember = new ChatUser
                {
                    ChatId = request.GroupId,
                    UserId = memberId
                };
                await _repository.AddGroupMemberAsync(groupMember, cancellationToken);
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);

        var updatedGroup = await _repository.GetGroupByIdWithMembersAsync(request.GroupId, cancellationToken);
        var membersList = updatedGroup!.Members.Select(m => m.UserId).ToList();

        return new GroupDto(
            updatedGroup.Id,
            updatedGroup.Name ?? string.Empty,
            updatedGroup.CreatorId ?? Guid.Empty,
            membersList,
            updatedGroup.CreatedAt,
            membersList.Count
        );
    }
}
