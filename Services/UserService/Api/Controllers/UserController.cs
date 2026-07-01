using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialApp.UserService.Application.Commands;
using SocialApp.UserService.Application.DTOs;
using SocialApp.UserService.Domain.Repositories;

namespace SocialApp.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserRepository _repository;

    public UserController(IMediator mediator, IUserRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    /// <summary>
    /// Lấy thông tin user theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "Không tìm thấy người dùng" });

        var dto = new UserProfileDto(
            user.Id, user.AuthUserId, user.Username, user.FirstName, user.LastName,
            user.Email, user.Phone, user.Avatar, user.Dob, user.Bio,
            user.Location, user.Website, user.IsActive, user.LastActiveAt,
            user.CreatedAt, user.UpdatedAt
        );

        return Ok(dto);
    }

    /// <summary>
    /// Cập nhật thông tin user
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var command = new UpdateUserCommand(id, dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thông tin user theo username
    /// </summary>
    [HttpGet("username/{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        var user = await _repository.GetByUsernameAsync(username);
        if (user == null)
            return NotFound(new { message = "Không tìm thấy người dùng" });

        var dto = new UserProfileDto(
            user.Id, user.AuthUserId, user.Username, user.FirstName, user.LastName,
            user.Email, user.Phone, user.Avatar, user.Dob, user.Bio,
            user.Location, user.Website, user.IsActive, user.LastActiveAt,
            user.CreatedAt, user.UpdatedAt
        );

        return Ok(dto);
    }
}
