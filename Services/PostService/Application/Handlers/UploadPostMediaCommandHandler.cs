using MediatR;
using SocialApp.PostService.Application.Commands;
using SocialApp.PostService.Domain.Entities;
using SocialApp.PostService.Domain.Repositories;

public class UploadPostMediaCommandHandler : IRequestHandler<UploadPostMediaCommand, bool>
{
    private readonly IPostRepository _repository;
    private readonly IFileStorageService _storageService;

    public UploadPostMediaCommandHandler(IPostRepository repository, IFileStorageService storageService)
    {
        _repository = repository;
        _storageService = storageService;
    }

    public async Task<bool> Handle(UploadPostMediaCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra bài viết tồn tại
        var post = await _repository.GetByIdAsync(request.PostId, cancellationToken);
        if (post == null) throw new KeyNotFoundException("Bài viết không tồn tại.");

        // 2. Bảo mật: Chỉ chủ bài viết mới được đăng ảnh lên bài của mình
        if (post.AuthorId != request.UserId) throw new UnauthorizedAccessException("Không có quyền chỉnh sửa bài viết.");

        // 3. Tiến hành upload lên Cloudinary thông qua Stream
        using var stream = request.File.OpenReadStream();
        var uploadResult = await _storageService.UploadAsync(stream, request.File.FileName, request.File.ContentType);

        // 4. Khởi tạo Entity PostMedia để lưu xuống database
        var media = new PostMedia
        {
            Id = Guid.NewGuid(),
            PostId = request.PostId,
            MediaUrl = uploadResult.Url,
            MediaType = request.File.ContentType.StartsWith("video") ? "Video" : "Image",
            MimeType = request.File.ContentType,
            FileSize = $"{uploadResult.FileSize / 1024} KB", // Chuyển đổi lưu chuỗi dạng KB
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddMediaAsync(media, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}