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

/// <summary>
/// Dữ liệu yêu cầu gửi tin nhắn
/// </summary>
/// <param name="Content">Nội dung tin nhắn</param>
/// <param name="ChatId">ID của cuộc trò chuyện (Private hoặc Group)</param>
public record SendMessageDto(
    string Content,
    Guid ChatId
);

/// <summary>
/// Dữ liệu yêu cầu tạo nhóm chat
/// </summary>
/// <param name="Name">Tên của nhóm chat</param>
/// <param name="Members">Danh sách các thành viên ban đầu (dạng GUID)</param>
public record CreateGroupDto(
    string Name,
    List<Guid> Members
);

/// <summary>
/// Dữ liệu yêu cầu thêm thành viên vào nhóm
/// </summary>
/// <param name="Members">Danh sách ID của các thành viên cần thêm (dạng GUID)</param>
public record AddMembersToGroupDto(
    List<Guid> Members
);

public record GroupMemberDto(
    Guid UserId
);

public record GroupDto(
    Guid GroupId,
    string Name,
    Guid CreatorId,
    List<Guid> Members,
    DateTime CreatedAt,
    int MemberCount
);

public record PrivateChatDto(
    Guid ChatId,
    string Type,
    List<Guid> Members,
    DateTime CreatedAt,
    bool IsExisting
);

/// <summary>
/// Dữ liệu yêu cầu tạo hoặc lấy cuộc trò chuyện riêng tư
/// </summary>
/// <param name="UserId1">ID của người dùng thứ nhất</param>
/// <param name="UserId2">ID của người dùng thứ hai</param>
public record CreateOrGetPrivateChatRequestDto(
    Guid UserId1,
    Guid UserId2
);

public record ChatListDto(
    Guid ChatId,
    string Type,
    string Name,
    List<Guid> Members,
    DateTime CreatedAt,
    bool IsActive
);

/// <summary>
/// Dữ liệu yêu cầu đổi tên nhóm chat
/// </summary>
/// <param name="Name">Tên mới cho nhóm chat</param>
public record RenameGroupDto(
    string Name
);
