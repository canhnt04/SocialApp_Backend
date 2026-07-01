using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialApp.ChatService.Application.Commands;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Domain.Repositories;

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
    /// Gửi tin nhắn (REST fallback)
    /// </summary>
    [HttpPost("messages")]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var command = new SendMessageCommand(
            dto.Content, dto.SenderId, dto.SenderUsername,
            dto.RecipientId, dto.ChatGroupId
        );
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Lấy tin nhắn giữa 2 người
    /// </summary>
    [HttpGet("messages/private")]
    [Authorize]
    public async Task<IActionResult> GetPrivateMessages(
        [FromQuery] Guid userId1, [FromQuery] Guid userId2,
        [FromQuery] int take = 50, [FromQuery] int skip = 0)
    {
        var messages = await _repository.GetMessagesBetweenUsersAsync(userId1, userId2, take, skip);
        var dtos = messages.Select(m => new MessageDto(
            m.Id, m.Content, m.SenderId, m.SenderUsername,
            m.RecipientId, m.ChatGroupId, m.IsRead, m.CreatedAt
        ));
        return Ok(dtos);
    }

    /// <summary>
    /// Lấy tin nhắn nhóm
    /// </summary>
    [HttpGet("messages/group/{groupId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetGroupMessages(
        Guid groupId, [FromQuery] int take = 50, [FromQuery] int skip = 0)
    {
        var messages = await _repository.GetGroupMessagesAsync(groupId, take, skip);
        var dtos = messages.Select(m => new MessageDto(
            m.Id, m.Content, m.SenderId, m.SenderUsername,
            m.RecipientId, m.ChatGroupId, m.IsRead, m.CreatedAt
        ));
        return Ok(dtos);
    }

    /// <summary>
    /// Tạo nhóm chat
    /// </summary>
    [HttpPost("groups")]
    [Authorize]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        var command = new CreateGroupCommand(
            dto.Name, dto.Description, dto.CreatedByUserId, dto.Members
        );
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách nhóm chat của user
    /// </summary>
    [HttpGet("groups/user/{userId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetUserGroups(Guid userId)
    {
        var groups = await _repository.GetUserGroupsAsync(userId);
        var dtos = groups.Select(g => new GroupDto(
            g.Id, g.Name, g.Description, g.CreatedByUserId,
            g.Members.Select(m => new GroupMemberDto(m.UserId, m.Username, m.IsAdmin)).ToList(),
            g.CreatedAt
        ));
        return Ok(dtos);
    }
}
