using MediatR;
using SocialApp.UserService.Domain.Repositories;

namespace SocialApp.UserService.Application.Features.GetUserById;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdResponse>
{
    private readonly IUserRepository _repository;

    public GetUserByIdHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetUserByIdResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng");

        return new GetUserByIdResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.Avatar,
            user.Dob,
            user.Bio,
            user.Location,
            user.Website
        );
    }
}
