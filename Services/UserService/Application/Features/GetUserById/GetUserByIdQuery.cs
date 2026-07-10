using MediatR;

namespace SocialApp.UserService.Application.Features.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IRequest<GetUserByIdResponse>;
