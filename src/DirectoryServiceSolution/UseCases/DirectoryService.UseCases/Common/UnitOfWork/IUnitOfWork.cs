using ResultLibrary;

namespace DirectoryService.UseCases.Common.UnitOfWork;

public interface IUnitOfWork
{
    Task<Result> SaveChanges(CancellationToken ct = default);
}