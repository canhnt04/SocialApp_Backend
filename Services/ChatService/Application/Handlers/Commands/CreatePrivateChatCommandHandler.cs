using MediatR;
using SocialApp.ChatService.Application.Commands;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Domain.Entities;
using SocialApp.ChatService.Domain.Repositories;

namespace SocialApp.ChatService.Application.Handlers.Commands;

public class CreateOrGetPrivateChatCommandHandler : IRequestHandler<CreateOrGetPrivateChatCommand, PrivateChatDto>
{
    private readonly IChatRepository _repository;

    public CreateOrGetPrivateChatCommandHandler(IChatRepository repository)
    {
        _repository = repository;
    }

    public async Task<PrivateChatDto> Handle(CreateOrGetPrivateChatCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation
        if (request.UserId1 == Guid.Empty || request.UserId2 == Guid.Empty)
        {
            throw new ArgumentException("User IDs cannot be empty.");
        }

        if (request.UserId1 == request.UserId2)
        {
            throw new ArgumentException("Cannot create a private chat with yourself.");
        }

        if (request.CurrentUserId != request.UserId1 && request.CurrentUserId != request.UserId2)
        {
            throw new UnauthorizedAccessException("You do not have permission to create this private chat.");
        }

        // 2. Chuẩn hóa thứ tự cặp (minUserId, maxUserId)
        var minUserId = request.UserId1.CompareTo(request.UserId2) < 0 ? request.UserId1 : request.UserId2;
        var maxUserId = request.UserId1.CompareTo(request.UserId2) < 0 ? request.UserId2 : request.UserId1;

        // 3. Bắt đầu transaction
        using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        try
        {
            // Kiểm tra xem chat riêng tư đã tồn tại chưa
            var existingChat = await _repository.GetPrivateChatByUserPairAsync(minUserId, maxUserId, cancellationToken);
            
            if (existingChat != null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new PrivateChatDto(
                    existingChat.Id,
                    "Private",
                    new List<Guid> { minUserId, maxUserId },
                    existingChat.CreatedAt,
                    true
                );
            }

            // Tạo chat riêng tư mới
            var privateKey = $"{minUserId}:{maxUserId}";
            var privateChat = new Chat
            {
                Type = ChatType.Private,
                IsActive = true,
                PrivateKey = privateKey
            };

            await _repository.AddGroupAsync(privateChat, cancellationToken);

            // Thêm cả hai người dùng vào chat
            var user1 = new ChatUser
            {
                ChatId = privateChat.Id,
                UserId = minUserId
            };
            var user2 = new ChatUser
            {
                ChatId = privateChat.Id,
                UserId = maxUserId
            };

            await _repository.AddGroupMemberAsync(user1, cancellationToken);
            await _repository.AddGroupMemberAsync(user2, cancellationToken);

            await _repository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new PrivateChatDto(
                privateChat.Id,
                "Private",
                new List<Guid> { minUserId, maxUserId },
                privateChat.CreatedAt,
                false
            );
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
