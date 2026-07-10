using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialApp.UserService.Application.Features.FollowUser;
using SocialApp.UserService.Application.Features.UnfollowUser;
using SocialApp.UserService.Application.Features.GetUserById;
using SocialApp.UserService.Application.Features.GetFollowers;
using SocialApp.UserService.Application.Features.GetFollowing;
using SocialApp.UserService.Application.Features.UpdateUser;
using SocialApp.UserService.Domain.Repositories;
using SocialApp.UserService.Infrastructure.Authentication;

namespace SocialApp.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _repository;

    public UserController(IMediator mediator, ICurrentUser currentUser, IUserRepository repository)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _repository = repository;
    }

    /// <summary>
    /// Lấy thông tin profile người dùng khác
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật thông tin profile của người dùng hiện tại
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromBody] UpdateUserCommand data, CancellationToken cancellationToken)
    {
        if (_currentUser.Id == Guid.Empty)
        {
            return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
        }

        try
        {
            var command = data with { UserId = _currentUser.Id ?? Guid.Empty };
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Follow người dùng khác
    /// </summary>
    [HttpPost("{targetUserId:guid}/follow")]
    [Authorize]
    public async Task<IActionResult> Follow(Guid targetUserId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new FollowUserCommand(targetUserId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Unfollow người dùng khác
    /// </summary>
    [HttpDelete("{targetUserId:guid}/follow")]
    [Authorize]
    public async Task<IActionResult> Unfollow(Guid targetUserId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new UnfollowUserCommand(targetUserId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách followers của user
    /// </summary>
    [HttpGet("{userId:guid}/followers")]
    public async Task<IActionResult> GetFollowers(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFollowersQuery(userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách following của user
    /// </summary>
    [HttpGet("{userId:guid}/following")]
    public async Task<IActionResult> GetFollowing(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFollowingQuery(userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách followers của user hiện tại
    /// </summary>
    [HttpGet("me/followers")]
    [Authorize]
    public async Task<IActionResult> GetMyFollowers(CancellationToken cancellationToken)
    {
        var authUserId = _currentUser.Id;
        if (authUserId is null)
        {
            return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
        }

        var profile = await _repository.GetByAuthUserIdAsync(authUserId.Value, cancellationToken);
        if (profile is null)
        {
            return NotFound(new { message = "Không tìm thấy hồ sơ người dùng" });
        }

        var result = await _mediator.Send(new GetFollowersQuery(profile.Id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách following của user hiện tại
    /// </summary>
    [HttpGet("me/following")]
    [Authorize]
    public async Task<IActionResult> GetMyFollowing(CancellationToken cancellationToken)
    {
        var authUserId = _currentUser.Id;
        if (authUserId is null)
        {
            return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
        }

        var profile = await _repository.GetByAuthUserIdAsync(authUserId.Value, cancellationToken);
        if (profile is null)
        {
            return NotFound(new { message = "Không tìm thấy hồ sơ người dùng" });
        }

        var result = await _mediator.Send(new GetFollowingQuery(profile.Id), cancellationToken);
        return Ok(result);
    }
}
