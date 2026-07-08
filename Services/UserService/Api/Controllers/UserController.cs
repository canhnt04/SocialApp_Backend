using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialApp.UserService.Application.Commands;
using SocialApp.UserService.Application.DTOs;
using SocialApp.UserService.Domain.Entities;
using SocialApp.UserService.Domain.Repositories;
using SocialApp.UserService.Infrastructure.Authentication;

namespace SocialApp.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserRepository _repository;
    private readonly ICurrentUser _currentUser;

    public UserController(IMediator mediator, IUserRepository repository, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _repository = repository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 👤 Lấy thông tin profile người dùng
    /// </summary>
    /// <remarks>
    /// Truy xuất thông tin profile đầy đủ của một người dùng.
    /// 
    /// **Thông tin trả về:**
    /// - Tên đầy đủ, avatar, bio
    /// - Địa chỉ, website
    /// - Trạng thái hoạt động (IsActive, LastActiveAt)
    /// - Thời gian tạo và cập nhật
    /// </remarks>
    /// <param name="id">ID của người dùng (GUID format)</param>
    /// <returns>Thông tin profile đầy đủ</returns>
    /// <response code="200">Lấy thành công</response>
    /// <response code="404">Không tìm thấy người dùng với ID này</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// ✏️ Cập nhật thông tin profile
    /// </summary>
    /// <remarks>
    /// Cập nhật các thông tin cá nhân của người dùng như:
    /// - Tên đầy đủ (FirstName, LastName)
    /// - Avatar URL
    /// - Bio / Tiểu sử
    /// - Địa chỉ, website
    /// - Ngày sinh
    /// 
    /// **Quy tắc:**
    /// - Chỉ chính người dùng (xác thực bằng JWT Token) mới có thể cập nhật profile của mình
    /// - ID trong URL phải khớp với ID người dùng hiện tại
    /// </remarks>
    /// <param name="id">ID của người dùng cần cập nhật</param>
    /// <param name="dto">Dữ liệu profile cần cập nhật</param>
    /// <returns>Thông tin profile đã cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="401">Không xác thực (thiếu/sai JWT Token)</response>
    /// <response code="403">Không có quyền cập nhật profile của người dùng khác</response>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// Lấy thông tin user hiện tại từ JWT
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var authUserId = _currentUser.Id;
        if (authUserId is null)
        {
            return Unauthorized(new { message = "JWT không hợp lệ hoặc thiếu claim sub" });
        }

        var user = await _repository.GetByAuthUserIdAsync(authUserId.Value, cancellationToken);
        if (user == null)
            return NotFound(new { message = "Không tìm thấy hồ sơ người dùng" });

        var dto = new UserProfileDto(
            user.Id, user.AuthUserId, user.Username, user.FirstName, user.LastName,
            user.Email, user.Phone, user.Avatar, user.Dob, user.Bio,
            user.Location, user.Website, user.IsActive, user.LastActiveAt,
            user.CreatedAt, user.UpdatedAt
        );

        return Ok(dto);
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
        var followers = await _repository.GetFollowersAsync(userId, cancellationToken);
        return Ok(followers.Select(ToDto));
    }

    /// <summary>
    /// Lấy danh sách following của user
    /// </summary>
    [HttpGet("{userId:guid}/following")]
    public async Task<IActionResult> GetFollowing(Guid userId, CancellationToken cancellationToken)
    {
        var following = await _repository.GetFollowingAsync(userId, cancellationToken);
        return Ok(following.Select(ToDto));
    }

    /// <summary>
    /// Lấy danh sách followers của user hiện tại
    /// </summary>
    [HttpGet("me/followers")]
    [Authorize]
    public async Task<IActionResult> GetMyFollowers(CancellationToken cancellationToken)
    {
        var profile = await GetCurrentProfile(cancellationToken);
        if (profile is null)
        {
            return NotFound(new { message = "Không tìm thấy hồ sơ người dùng" });
        }

        var followers = await _repository.GetFollowersAsync(profile.Id, cancellationToken);
        return Ok(followers.Select(ToDto));
    }

    /// <summary>
    /// Lấy danh sách following của user hiện tại
    /// </summary>
    [HttpGet("me/following")]
    [Authorize]
    public async Task<IActionResult> GetMyFollowing(CancellationToken cancellationToken)
    {
        var profile = await GetCurrentProfile(cancellationToken);
        if (profile is null)
        {
            return NotFound(new { message = "Không tìm thấy hồ sơ người dùng" });
        }

        var following = await _repository.GetFollowingAsync(profile.Id, cancellationToken);
        return Ok(following.Select(ToDto));
    }

    private async Task<UserProfile?> GetCurrentProfile(CancellationToken cancellationToken)
    {
        var authUserId = _currentUser.Id;
        if (authUserId is null)
        {
            return null;
        }

        return await _repository.GetByAuthUserIdAsync(authUserId.Value, cancellationToken);
    }

    private static UserProfileDto ToDto(UserProfile user) => new(
        user.Id, user.AuthUserId, user.Username, user.FirstName, user.LastName,
        user.Email, user.Phone, user.Avatar, user.Dob, user.Bio,
        user.Location, user.Website, user.IsActive, user.LastActiveAt,
        user.CreatedAt, user.UpdatedAt
    );
}
