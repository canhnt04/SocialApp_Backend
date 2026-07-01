using Microsoft.EntityFrameworkCore;
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

    // Messages
    public async Task<Message?> GetMessageByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Messages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(
        Guid userId1, Guid userId2, int take = 50, int skip = 0, CancellationToken cancellationToken = default)
        => await _context.Messages
            .Where(m => m.ChatId != Guid.Empty && 
                (m.Chat.Type == ChatType.Private && m.Chat.Members.Any(u => u.UserId == userId1) && m.Chat.Members.Any(u => u.UserId == userId2)))
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Message>> GetGroupMessagesAsync(
        Guid groupId, int take = 50, int skip = 0, CancellationToken cancellationToken = default)
        => await _context.Messages
            .Where(m => m.ChatId == groupId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task AddMessageAsync(Message message, CancellationToken cancellationToken = default)
        => await _context.Messages.AddAsync(message, cancellationToken);

    // Groups
    public async Task<Chat?> GetGroupByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Chats
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<IEnumerable<Chat>> GetUserGroupsAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Chats
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);

    public async Task AddGroupAsync(Chat group, CancellationToken cancellationToken = default)
        => await _context.Chats.AddAsync(group, cancellationToken);

    public async Task AddGroupMemberAsync(ChatUser member, CancellationToken cancellationToken = default)
        => await _context.ChatUsers.AddAsync(member, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
