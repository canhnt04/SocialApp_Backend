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
            .Where(m => m.ChatGroupId == null &&
                ((m.SenderId == userId1 && m.RecipientId == userId2) ||
                 (m.SenderId == userId2 && m.RecipientId == userId1)))
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Message>> GetGroupMessagesAsync(
        Guid groupId, int take = 50, int skip = 0, CancellationToken cancellationToken = default)
        => await _context.Messages
            .Where(m => m.ChatGroupId == groupId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task AddMessageAsync(Message message, CancellationToken cancellationToken = default)
        => await _context.Messages.AddAsync(message, cancellationToken);

    // Groups
    public async Task<ChatGroup?> GetGroupByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.ChatGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<IEnumerable<ChatGroup>> GetUserGroupsAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.ChatGroups
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);

    public async Task AddGroupAsync(ChatGroup group, CancellationToken cancellationToken = default)
        => await _context.ChatGroups.AddAsync(group, cancellationToken);

    public async Task AddGroupMemberAsync(ChatGroupMember member, CancellationToken cancellationToken = default)
        => await _context.ChatGroupMembers.AddAsync(member, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
