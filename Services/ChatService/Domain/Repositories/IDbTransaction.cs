using System;
using System.Threading;
using System.Threading.Tasks;

namespace SocialApp.ChatService.Domain.Repositories;

public interface IDbTransaction : IDisposable, IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
