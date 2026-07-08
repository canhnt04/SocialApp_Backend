using MediatR;
using SocialApp.ChatService.Application.Commands;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Domain.Entities;
using SocialApp.ChatService.Domain.Repositories;

namespace SocialApp.ChatService.Application.Handlers.Commands;

public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, GroupDto>
{
    private readonly IChatRepository _repository;

    public CreateGroupCommandHandler(IChatRepository repository)
    {
        _repository = repository;
    }

    public async Task<GroupDto> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Group name cannot be empty.");
        }

        if (request.CreatorId == Guid.Empty)
        {
            throw new ArgumentException("Group creator must be specified.");
        }

        // 2. Chuẩn hóa danh sách thành viên (deduplicate, add creator, remove empty)
        var memberList = request.Members ?? new List<Guid>();
        var cleanMembers = memberList
            .Where(id => id != Guid.Empty)
            .ToList();

        if (!cleanMembers.Contains(request.CreatorId))
        {
            cleanMembers.Add(request.CreatorId);
        }

        cleanMembers = cleanMembers.Distinct().ToList();

        // Kiểm tra số lượng thành viên tối thiểu là 3 (bao gồm creator)
        if (cleanMembers.Count < 3)
        {
            throw new ArgumentException("A group must have at least 3 members (including the creator).");
        }

        // 3. Bắt đầu transaction
        using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        try
        {
            var group = new Chat
            {
                Name = request.Name.Trim(),
                Type = ChatType.Group,
                CreatorId = request.CreatorId,
                IsActive = true
            };

            await _repository.AddGroupAsync(group, cancellationToken);

            // Thêm tất cả các thành viên vào nhóm
            foreach (var memberId in cleanMembers)
            {
                var groupMember = new ChatUser
                {
                    ChatId = group.Id,
                    UserId = memberId
                };
                await _repository.AddGroupMemberAsync(groupMember, cancellationToken);
            }

            await _repository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new GroupDto(
                group.Id,
                group.Name,
                group.CreatorId.Value,
                cleanMembers,
                group.CreatedAt,
                cleanMembers.Count
            );
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
