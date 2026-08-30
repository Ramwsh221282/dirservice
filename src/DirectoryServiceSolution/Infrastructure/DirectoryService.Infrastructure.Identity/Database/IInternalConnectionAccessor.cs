using System.Data;

namespace DirectoryService.Infrastructure.Identity.Database;

/// <summary>
/// Внутренний контракт, дающий репозиториям доступ к соединению внутри транзакционной области.
/// Не должен использоваться за пределами сборки - для этого предназначен публичный
/// <see cref="IIdentityTransactionScope"/>, у которого нет доступа к соединению напрямую.
/// </summary>
internal interface IInternalConnectionAccessor
{
    IDbConnection Connection { get; }
}
