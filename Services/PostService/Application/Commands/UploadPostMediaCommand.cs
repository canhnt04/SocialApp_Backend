using MediatR;
using Microsoft.AspNetCore.Http;

namespace SocialApp.PostService.Application.Commands;

public record UploadPostMediaCommand(
    Guid PostId,
    Guid UserId,
    IFormFile File
) : IRequest<bool>;