using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using SocialApp.ChatService.Domain.Repositories;

namespace SocialApp.ChatService.Infrastructure.Repositories;

public class DbTransactionAdapter : IDbTransaction
{
    private readonly IDbContextTransaction _efTransaction;

    public DbTransactionAdapter(IDbContextTransaction efTransaction)
    {
        _efTransaction = efTransaction;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => _efTransaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => _efTransaction.RollbackAsync(cancellationToken);

    public void Dispose()
        => _efTransaction.Dispose();

    public ValueTask DisposeAsync()
        => _efTransaction.DisposeAsync();
}
