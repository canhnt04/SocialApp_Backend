using SocialApp.ChatService.Domain.Entities;

namespace SocialApp.ChatService.Domain.Repositories;

public interface IChatRepository
{
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    // Messages
    Task<Message?> GetMessageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Message>> GetChatMessagesAsync(Guid chatId, int take = 50, int skip = 0, CancellationToken cancellationToken = default);
    Task AddMessageAsync(Message message, CancellationToken cancellationToken = default);

    // Chats & Groups
    Task<Chat?> GetChatByIdAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task<Chat?> GetGroupByIdWithMembersAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chat>> GetUserGroupsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId, int take = 50, int skip = 0, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatUser>> GetChatMembersAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task<bool> IsUserInChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default);
    Task AddGroupAsync(Chat group, CancellationToken cancellationToken = default);
    Task AddGroupMemberAsync(ChatUser member, CancellationToken cancellationToken = default);
    Task RemoveGroupMemberAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default);

    // Private Chat
    Task<Chat?> GetPrivateChatByUserPairAsync(Guid minUserId, Guid maxUserId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
