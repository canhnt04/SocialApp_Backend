using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SocialApp.ChatService.Domain.Entities;
using SocialApp.ChatService.Domain.Repositories;
using SocialApp.ChatService.Infrastructure.Data;

namespace SocialApp.ChatService.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly ChatDbContext _context;

    public ChatRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var efTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new DbTransactionAdapter(efTransaction);
    }

    // Messages
    public async Task<Message?> GetMessageByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Messages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IEnumerable<Message>> GetChatMessagesAsync(Guid chatId, int take = 50, int skip = 0, CancellationToken cancellationToken = default)
        => await _context.Messages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task AddMessageAsync(Message message, CancellationToken cancellationToken = default)
        => await _context.Messages.AddAsync(message, cancellationToken);

    // Chats & Groups
    public async Task<Chat?> GetChatByIdAsync(Guid chatId, CancellationToken cancellationToken = default)
        => await _context.Chats
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);

    public async Task<Chat?> GetGroupByIdWithMembersAsync(Guid groupId, CancellationToken cancellationToken = default)
        => await _context.Chats
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.Type == ChatType.Group, cancellationToken);

    public async Task<IEnumerable<Chat>> GetUserGroupsAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Chats
            .Include(g => g.Members)
            .Where(g => g.Type == ChatType.Group && g.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId, int take = 50, int skip = 0, CancellationToken cancellationToken = default)
        => await _context.Chats
            .Include(c => c.Members)
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .OrderByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ChatUser>> GetChatMembersAsync(Guid chatId, CancellationToken cancellationToken = default)
        => await _context.ChatUsers
            .Where(cu => cu.ChatId == chatId)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsUserInChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
        => await _context.ChatUsers
            .AnyAsync(x => x.ChatId == chatId && x.UserId == userId, cancellationToken);

    public async Task AddGroupAsync(Chat group, CancellationToken cancellationToken = default)
        => await _context.Chats.AddAsync(group, cancellationToken);

    public async Task AddGroupMemberAsync(ChatUser member, CancellationToken cancellationToken = default)
        => await _context.ChatUsers.AddAsync(member, cancellationToken);

    public async Task RemoveGroupMemberAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.ChatUsers.FirstOrDefaultAsync(cu => cu.ChatId == chatId && cu.UserId == userId, cancellationToken);
        if (member != null)
        {
            _context.ChatUsers.Remove(member);
        }
    }

    public async Task<Chat?> GetPrivateChatByUserPairAsync(Guid minUserId, Guid maxUserId, CancellationToken cancellationToken = default)
        => await _context.Chats
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Type == ChatType.Private &&
                c.Members.Any(m => m.UserId == minUserId) &&
                c.Members.Any(m => m.UserId == maxUserId), cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
