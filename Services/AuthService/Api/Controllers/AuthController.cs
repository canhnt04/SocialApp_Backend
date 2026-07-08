using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialApp.AuthService.Application.Commands;
using SocialApp.AuthService.Application.DTOs.Requests;

namespace SocialApp.AuthService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 🔐 Đăng ký tài khoản mới
    /// </summary>
    /// <remarks>
    /// Tạo một tài khoản người dùng mới trong hệ thống.
    /// 
    /// **Quy tắc kiểm tra:**
    /// - Username phải duy nhất, không chứa ký tự đặc biệt
    /// - Email phải hợp lệ và duy nhất
    /// - Mật khẩu tối thiểu 6 ký tự
    /// - Phone tùy chọn, phải là số hợp lệ
    /// 
    /// **Quy trình xử lý:**
    /// 1. Kiểm tra username/email đã tồn tại
    /// 2. Mã hóa mật khẩu bằng BCrypt
    /// 3. Lưu user vào database
    /// 4. Phát event để UserService tạo profile
    /// 5. Trả về JWT token &amp; RefreshToken
    /// </remarks>
    /// <param name="request">Thông tin đăng ký gồm Username, Email, Phone (tùy chọn), Password</param>
    /// <returns>JWT Token và RefreshToken nếu thành công</returns>
    /// <response code="200">Đăng ký thành công, trả về tokens</response>
    /// <response code="400">Dữ liệu không hợp lệ hoặc email/username đã tồn tại</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterUserCommand(request.Username, request.Email, request.Phone, request.Password);

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginUserCommand(request.UsernameOrEmail, request.Password);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Làm mới token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
