using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Transaction;
using DirectoryService.UseCases.Common.UnitOfWork;
using DirectoryService.UseCases.Departments.Contracts;
using ResultLibrary;

namespace DirectoryService.UseCases.Departments.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand<Guid>;

public sealed class DeleteDepartmentHandler : ICommandHandler<Guid, DeleteDepartmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDepartmentsRepository _repository;
    private readonly ITransactionSource _transactionSource;

    public DeleteDepartmentHandler(
        IUnitOfWork unitOfWork, 
        IDepartmentsRepository repository, 
        ITransactionSource transactionSource)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
        _transactionSource = transactionSource;
    }

    public async Task<Result<Guid>> Handle(DeleteDepartmentCommand command, CancellationToken ct = default)
    {
        await using ITransactionScope txn = await _transactionSource.ReceiveTransaction(ct);

        Result<Department> department = await FindDepartment(command.Id, ct);        
        if (department.IsFailure)
        {
            return department.Error;
        }
        
        DepartmentPath copied = department.Value.Path.Copy();
        Result archivation = department.Value.Archive();
        if (archivation.IsFailure)
        {
            return archivation.Error;
        }        

        Result locationsArchivation = await ArchiveLocationsOnlyOwnedByDepartment(department, ct);
        if (locationsArchivation.IsFailure)
        {
            return locationsArchivation.Error;
        }

        Result positionsArchivation = await ArchivePositionsOnlyOwnedByDepartment(department, ct);
        if (positionsArchivation.IsFailure)
        {
            return positionsArchivation.Error;
        }

        await _repository.RefreshDepartmentChildPaths(department, copied, ct);

        Result saving = await _unitOfWork.SaveChanges(ct: ct);
        if (saving.IsFailure)
        {
            return saving.Error;
        }

        Result commiting = await txn.CommitChanges(nameof(DeleteDepartmentCommand), ct: ct);
        if (commiting.IsFailure)
        {
            return commiting.Error;
        }

        return department.Value.Id.Value;
    }

    private async Task<Result<Department>> FindDepartment(Guid id, CancellationToken ct)
    {
        Result<Department> department = await _repository.GetById(id, useLock: true, ct);        
        return department;
    }

    private async Task<Result> ArchiveLocationsOnlyOwnedByDepartment(Department department, CancellationToken ct)
    {        
        foreach (DepartmentLocation loc in await _repository.GetSingleTimeAttachedDepartmentLocations(department, ct))
        {
            Result archivation = loc.Archive();
            if (archivation.IsFailure)
            {
                return archivation.Error;
            }
        }

        return Result.Success();
    }

    private async Task<Result> ArchivePositionsOnlyOwnedByDepartment(Department department, CancellationToken ct)
    {
        foreach (DepartmentPosition pos in await _repository.GetSingleTimeAttachedDepartmentPositions(department, ct))
        {
            Result archivation = pos.Archive();
            if (archivation.IsFailure)
            {
                return archivation.Error;
            }
        }

        return Result.Success();
    }
}
