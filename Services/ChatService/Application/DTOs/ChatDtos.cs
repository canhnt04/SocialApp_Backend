namespace SocialApp.ChatService.Application.DTOs;

public record MessageDto(
    Guid Id,
    string Content,
    Guid SenderId,
    string SenderUsername,
    Guid ChatId,
    bool IsRead,
    DateTime CreatedAt
);

public record SendMessageDto(
    string Content,
    Guid SenderId,
    string SenderUsername,
    Guid ChatId
);

public record CreateGroupDto(
    string Name,
    List<GroupMemberDto> Members
);

public record GroupMemberDto(
    Guid UserId
);

public record GroupDto(
    Guid Id,
    string Name,
    List<GroupMemberDto> Members,
    DateTime CreatedAt
);
