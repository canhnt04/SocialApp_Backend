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
        var group = new ChatGroup
        {
            Name = request.Name,
            Description = request.Description,
            CreatedByUserId = request.CreatedByUserId
        };

        await _repository.AddGroupAsync(group, cancellationToken);

        // Thêm thành viên
        foreach (var member in request.Members)
        {
            var groupMember = new ChatGroupMember
            {
                ChatGroupId = group.Id,
                UserId = member.UserId,
                Username = member.Username,
                IsAdmin = member.IsAdmin
            };
            await _repository.AddGroupMemberAsync(groupMember, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return new GroupDto(
            group.Id, group.Name, group.Description, group.CreatedByUserId,
            request.Members, group.CreatedAt
        );
    }
}
