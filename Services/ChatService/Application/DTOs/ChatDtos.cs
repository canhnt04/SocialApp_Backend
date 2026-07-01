namespace SocialApp.ChatService.Application.DTOs;

public record MessageDto(
    Guid Id,
    string Content,
    Guid SenderId,
    string SenderUsername,
    Guid? RecipientId,
    Guid? ChatGroupId,
    bool IsRead,
    DateTime CreatedAt
);

public record SendMessageDto(
    string Content,
    Guid SenderId,
    string SenderUsername,
    Guid? RecipientId,
    Guid? ChatGroupId
);

public record CreateGroupDto(
    string Name,
    string? Description,
    Guid CreatedByUserId,
    List<GroupMemberDto> Members
);

public record GroupMemberDto(
    Guid UserId,
    string Username,
    bool IsAdmin = false
);

public record GroupDto(
    Guid Id,
    string Name,
    string? Description,
    Guid CreatedByUserId,
    List<GroupMemberDto> Members,
    DateTime CreatedAt
);
