using MediatR;
using SocialApp.ChatService.Application.Commands;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Domain.Entities;
using SocialApp.ChatService.Domain.Repositories;

namespace SocialApp.ChatService.Application.Handlers;

public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, GroupDto>
{
    private readonly IChatRepository _repository;

    public CreateGroupCommandHandler(IChatRepository repository)
    {
        _repository = repository;
    }

    public async Task<GroupDto> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = new Chat
        {
            Name = request.Name,
            Type = ChatType.Group
        };

        await _repository.AddGroupAsync(group, cancellationToken);

        // Thêm thành viên
        foreach (var member in request.Members)
        {
            var groupMember = new ChatUser
            {
                ChatId = group.Id,
                UserId = member.UserId
            };
            await _repository.AddGroupMemberAsync(groupMember, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return new GroupDto(
            group.Id, group.Name,
            request.Members, group.CreatedAt
        );
    }
}
