using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Application.DTOs;
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
        var command = new CreatePostCommand(
            dto.AuthorId, dto.AuthorUsername, dto.Content,
            dto.ImageUrl, dto.VideoUrl, dto.Visibility
        );
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
            post.ImageUrl, post.VideoUrl, post.LikeCount, post.CommentCount,
            post.ShareCount, post.IsPublished, post.Visibility,
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
            p.ImageUrl, p.VideoUrl, p.LikeCount, p.CommentCount,
            p.ShareCount, p.IsPublished, p.Visibility,
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
            p.ImageUrl, p.VideoUrl, p.LikeCount, p.CommentCount,
            p.ShareCount, p.IsPublished, p.Visibility,
            p.CreatedAt, p.UpdatedAt
        ));
        return Ok(dtos);
    }

    /// <summary>
    /// Xóa bài viết
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var post = await _repository.GetByIdAsync(id);
        if (post == null)
            return NotFound(new { message = "Không tìm thấy bài viết" });

        _repository.Delete(post);
        await _repository.SaveChangesAsync();
        return NoContent();
    }
}
