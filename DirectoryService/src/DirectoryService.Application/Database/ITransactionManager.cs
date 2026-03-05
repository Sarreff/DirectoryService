using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Database;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        CancellationToken cancellationToken = default, IsolationLevel? level = null);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}