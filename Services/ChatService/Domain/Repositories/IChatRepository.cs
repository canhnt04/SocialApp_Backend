using SocialApp.ChatService.Domain.Entities;

namespace SocialApp.ChatService.Domain.Repositories;

public interface IChatRepository
{
    // Messages
    Task<Message?> GetMessageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(Guid userId1, Guid userId2, int take = 50, int skip = 0, CancellationToken cancellationToken = default);
    Task<IEnumerable<Message>> GetGroupMessagesAsync(Guid groupId, int take = 50, int skip = 0, CancellationToken cancellationToken = default);
    Task AddMessageAsync(Message message, CancellationToken cancellationToken = default);

    // Groups
    Task<ChatGroup?> GetGroupByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatGroup>> GetUserGroupsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddGroupAsync(ChatGroup group, CancellationToken cancellationToken = default);
    Task AddGroupMemberAsync(ChatGroupMember member, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
