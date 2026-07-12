using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Application.DTOs;
using SocialApp.PostService.Application.Queries;
using SocialApp.PostService.Domain.Entities;
using SocialApp.PostService.Domain.Repositories;

namespace SocialApp.PostService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PostController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPostRepository _repository;

    public PostController(IMediator mediator, IPostRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    /// <summary>
    /// Tạo bài viết mới
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePostDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn." });
        }
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("unique_name") ?? "Unknown";

        var command = new CreatePostCommand(
            userId,
            username,
            dto.Content,
            dto.ImageUrl,
            dto.VideoUrl,
            dto.Visibility
        );
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Chỉnh sửa nội dung và quyền riêng tư bài viết (Chỉ chủ sở hữu)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostInputModel model)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var command = new UpdatePostCommand(id, userId, model.Content, model.Visibility);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Xóa bài viết (Soft Delete - Chỉ chủ sở hữu)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var command = new SoftDeletePostCommand(id, userId);
        var result = await _mediator.Send(command);

        if (result)
        {
            return Ok(new { message = "Xóa bài viết thành công." });
        }

        return BadRequest("Không thể xóa bài viết.");
    }

    public class UpdatePostInputModel
    {
        public string Content { get; set; } = string.Empty;
        public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    }

    /// <summary>
    /// Lấy bài viết theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var post = await _repository.GetByIdAsync(id);
        if (post == null)
            return NotFound(new { message = "Không tìm thấy bài viết" });

        var dto = new PostDto(
            post.Id, post.AuthorId, post.AuthorUsername, post.Content,
            null, null, post.LikeCount, post.CommentCount,
            post.ShareCount, post.IsActive, post.Visibility,
            post.CreatedAt, post.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Lấy bài viết theo tác giả
    /// </summary>
    [HttpGet("author/{authorId:guid}")]
    public async Task<IActionResult> GetByAuthor(
        Guid authorId, [FromQuery] int take = 20, [FromQuery] int skip = 0)
    {
        var posts = await _repository.GetByAuthorIdAsync(authorId, take, skip);
        var dtos = posts.Select(p => new PostDto(
            p.Id, p.AuthorId, p.AuthorUsername, p.Content,
            null, null, p.LikeCount, p.CommentCount,
            p.ShareCount, p.IsActive, p.Visibility,
            p.CreatedAt, p.UpdatedAt
        ));
        return Ok(dtos);
    }

    /// <summary>
    /// Lấy bảng tin (feed) công khai
    /// </summary>
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed(
        [FromQuery] int take = 20, [FromQuery] int skip = 0)
    {
        var posts = await _repository.GetFeedAsync(take, skip);
        var dtos = posts.Select(p => new PostDto(
            p.Id, p.AuthorId, p.AuthorUsername, p.Content,
            null, null, p.LikeCount, p.CommentCount,
            p.ShareCount, p.IsActive, p.Visibility,
            p.CreatedAt, p.UpdatedAt
        ));
        return Ok(dtos);
    }

    /// <summary>
    /// Upload media (hình ảnh hoặc video) cho bài viết
    /// </summary>
    [HttpPost("{postId}/media")]
    [Authorize]
    public async Task<IActionResult> UploadMedia(Guid postId, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File không hợp lệ.");

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var result = await _mediator.Send(new UploadPostMediaCommand(postId, userId, file));

        if (result) return Ok(new { message = "Tải file lên bài viết thành công!" });
        return BadRequest("Không thể tải file lên.");
    }

    /// <summary>
    /// Share bài viết theo ID
    /// </summary>
    [HttpPost("{postId}/share")]
    [Authorize] // Bắt buộc đăng nhập để share bài
    public async Task<IActionResult> SharePost(Guid postId, [FromBody] SharePostInputModel model)
    {
        // Lấy thông tin user từ JWT token bảo mật
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }

        // Lấy thêm username từ token (giả sử auth service có lưu unique_name)
        var username = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

        var result = await _mediator.Send(new SharePostCommand(postId, userId, username, model.Content));

        return Ok(new { message = "Chia sẻ bài viết thành công!", data = result });
    }

    // Model phục vụ nhập nội dung kèm theo khi share
    public class SharePostInputModel
    {
        public string? Content { get; set; }
    }

    /// <summary>
    /// Like hoặc Unlike bài viết
    /// </summary>
    [HttpPost("{id:guid}/like")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(Guid id, [FromBody] LikePostDto dto)
    {
        var post = await _repository.GetByIdAsync(id);
        if (post == null)
            return NotFound(new { message = "Không tìm thấy bài viết" });

        var command = new LikePostCommand(id, dto.AuthorId);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Thêm bình luận mới
    /// </summary>
    [HttpPost("{postId}/comments")]
    [Authorize]
    public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentDto dto)
    {
        // Trích xuất UserId tự động từ JWT Token để bảo mật hơn thay vì tin cậy dto đầu vào
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid authorId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new CreateCommentCommand(postId, authorId, dto));
        return CreatedAtAction(nameof(GetComments), new { postId = postId }, result);
    }

    /// <summary>
    /// Lấy danh sách bình luận của bài viết
    /// </summary>
    [HttpGet("{postId}/comments")]
    public async Task<IActionResult> GetComments(Guid postId)
    {
        var result = await _mediator.Send(new GetCommentsByPostIdQuery(postId));
        return Ok(result);
    }

    /// <summary>
    /// Chỉnh sửa bình luận
    /// </summary>
    [HttpPut("comments/{commentId}")]
    [Authorize]
    public async Task<IActionResult> EditComment(Guid commentId, [FromBody] UpdateCommentInputModel model)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var result = await _mediator.Send(new UpdateCommentCommand(commentId, userId, model.Content));
        return Ok(result);
    }

    /// <summary>
    /// Xóa bình luận (soft delete)
    /// </summary>
    [HttpDelete("comments/{commentId}")]
    [Authorize] // Bắt buộc đăng nhập
    public async Task<IActionResult> SoftDeleteComment(Guid commentId)
    {
        // Lấy UserId từ JWT Token bảo mật
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new DeleteCommentCommand(commentId, userId));

        if (result)
        {
            return Ok(new { message = "Xóa bình luận thành công." });
        }

        return BadRequest("Không thể xóa bình luận.");
    }


    public class UpdateCommentInputModel
    {
        public string Content { get; set; } = string.Empty;
    }
}
