using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SocialApp.ChatService.Application.Commands;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Application.Queries;
using SocialApp.ChatService.Domain.Repositories;
using SocialApp.ChatService.Domain.Entities;

namespace SocialApp.ChatService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IChatRepository _repository;

    public ChatController(IMediator mediator, IChatRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    /// <summary>
    /// Tạo hoặc lấy chat riêng tư giữa hai người
    /// </summary>
    [HttpPost("private/create-or-get")]
    [Authorize]
    public async Task<IActionResult> CreateOrGetPrivateChat([FromBody] CreateOrGetPrivateChatRequestDto dto)
    {
        if (!TryGetCurrentUser(out var currentUserId, out _))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        if (dto.UserId1 == Guid.Empty || dto.UserId2 == Guid.Empty)
        {
            return BadRequest("User IDs cannot be empty.");
        }

        if (dto.UserId1 == dto.UserId2)
        {
            return BadRequest("Cannot create a private chat with yourself.");
        }

        if (currentUserId != dto.UserId1 && currentUserId != dto.UserId2)
        {
            return StatusCode(403, "You do not have permission to create this private chat.");
        }

        var command = new CreateOrGetPrivateChatCommand(dto.UserId1, dto.UserId2, currentUserId);
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
    }

    /// <summary>
    /// Gửi tin nhắn (REST fallback)
    /// </summary>
    [HttpPost("messages")]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        if (!TryGetCurrentUser(out var currentUserId, out var currentUsername))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        var command = new SendMessageCommand(
            dto.Content, currentUserId, currentUsername,
            dto.ChatId
        );
        
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Lấy danh sách tin nhắn theo chatId (phân trang, sắp xếp giảm dần theo thời gian tạo)
    /// </summary>
    [HttpGet("{chatId:guid}/messages")]
    [Authorize]
    public async Task<IActionResult> GetChatMessages(
        Guid chatId, [FromQuery] int take = 50, [FromQuery] int skip = 0)
    {
        if (!TryGetCurrentUser(out var currentUserId, out _))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        var query = new GetChatMessagesQuery(chatId, currentUserId, take, skip);
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
    }

    /// <summary>
    /// Lấy danh sách các cuộc trò chuyện của user đăng nhập
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyChats([FromQuery] int take = 50, [FromQuery] int skip = 0)
    {
        if (!TryGetCurrentUser(out var currentUserId, out _))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        var query = new GetMyChatsQuery(currentUserId, take, skip);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Đổi tên nhóm chat. Chỉ creator mới có quyền đổi tên
    /// </summary>
    [HttpPatch("groups/{groupId:guid}/rename")]
    [Authorize]
    public async Task<IActionResult> RenameGroup(Guid groupId, [FromBody] RenameGroupDto dto)
    {
        if (!TryGetCurrentUser(out var currentUserId, out _))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Group name cannot be empty.");
        }

        var chat = await _repository.GetChatByIdAsync(groupId);
        if (chat == null)
        {
            return NotFound("Group chat not found.");
        }

        if (chat.Type != ChatType.Group)
        {
            return BadRequest("Only group chats can be renamed.");
        }

        if (chat.CreatorId != currentUserId)
        {
            return StatusCode(403, "Only the group creator can rename this group.");
        }

        chat.Name = dto.Name.Trim();
        await _repository.SaveChangesAsync();

        return Ok(new GroupDto(
            chat.Id,
            chat.Name,
            chat.CreatorId.Value,
            chat.Members.Select(m => m.UserId).ToList(),
            chat.CreatedAt,
            chat.Members.Count
        ));
    }

    /// <summary>
    /// Vô hiệu hóa phòng chat. Chỉ creator (Group) hoặc member (Private) mới được thực hiện
    /// </summary>
    [HttpPatch("{chatId:guid}/deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateChat(Guid chatId)
    {
        if (!TryGetCurrentUser(out var currentUserId, out _))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
        {
            return NotFound("Chat room not found.");
        }

        if (chat.Type == ChatType.Group)
        {
            if (chat.CreatorId != currentUserId)
            {
                return StatusCode(403, "Only the group creator can deactivate this group.");
            }
        }
        else
        {
            var isMember = chat.Members.Any(m => m.UserId == currentUserId);
            if (!isMember)
            {
                return StatusCode(403, "You do not have permission to deactivate this private chat.");
            }
        }

        chat.IsActive = false;
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Rời khỏi nhóm chat
    /// </summary>
    [HttpPost("groups/{groupId:guid}/leave")]
    [Authorize]
    public async Task<IActionResult> LeaveGroup(Guid groupId)
    {
        if (!TryGetCurrentUser(out var currentUserId, out _))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        var chat = await _repository.GetChatByIdAsync(groupId);
        if (chat == null)
        {
            return NotFound("Group chat not found.");
        }

        if (chat.Type != ChatType.Group)
        {
            return BadRequest("You can only leave group chats.");
        }

        var isMember = chat.Members.Any(m => m.UserId == currentUserId);
        if (!isMember)
        {
            return BadRequest("You are not a member of this group.");
        }

        await _repository.RemoveGroupMemberAsync(groupId, currentUserId);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Tạo nhóm chat
    /// </summary>
    [HttpPost("groups")]
    [Authorize]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        if (!TryGetCurrentUser(out var currentUserId, out _))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        var command = new CreateGroupCommand(
            dto.Name, dto.Members, currentUserId
        );
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Thêm thành viên vào nhóm
    /// </summary>
    [HttpPost("groups/{groupId:guid}/members")]
    [Authorize]
    public async Task<IActionResult> AddMembersToGroup(Guid groupId, [FromBody] AddMembersToGroupDto dto)
    {
        if (!TryGetCurrentUser(out var currentUserId, out _))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        var isMember = await _repository.IsUserInChatAsync(groupId, currentUserId);
        if (!isMember)
        {
            return StatusCode(403, "Only group members can add new members.");
        }

        var command = new AddMembersToGroupCommand(groupId, dto.Members);
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Lấy danh sách nhóm chat của user
    /// </summary>
    [HttpGet("groups/user/{userId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetUserGroups(Guid userId)
    {
        if (!TryGetCurrentUser(out var currentUserId, out _))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        if (currentUserId != userId)
        {
            return Forbid();
        }

        var query = new GetUserGroupsQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("groups/me")]
    [Authorize]
    public async Task<IActionResult> GetMyGroups()
    {
        if (!TryGetCurrentUser(out var currentUserId, out _))
        {
            return Unauthorized("Missing or invalid user claims.");
        }

        var query = new GetUserGroupsQuery(currentUserId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private bool TryGetCurrentUser(out Guid userId, out string username)
    {
        userId = Guid.Empty;
        username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "unknown";

        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

        return Guid.TryParse(rawUserId, out userId) && userId != Guid.Empty;
    }
}
