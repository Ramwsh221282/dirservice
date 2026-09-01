using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Transaction;
using DirectoryService.UseCases.Departments.Contracts;
using ResultLibrary;

namespace DirectoryService.UseCases.Departments.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand<Guid>;

public sealed class DeleteDepartmentHandler : ICommandHandler<Guid, DeleteDepartmentCommand>
{
    private readonly IDepartmentsRepository _repository;
    private readonly ITransactionSource _transactionSource;

    public DeleteDepartmentHandler(
        IDepartmentsRepository repository,
        ITransactionSource transactionSource)
    {
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

        Result<Department> parent = await FindParent(department.Value, ct);
        if (parent.IsFailure)
        {
            return parent.Error;
        }

        Result archivation = department.Value.Archive();
        if (archivation.IsFailure)
        {
            return archivation.Error;
        }

        await _repository.Update(department.Value, ct);

        if (department.Value.Parent != null)
        {
            Result detaching = parent.Value.Detach(department.Value);
            if (detaching.IsFailure)
            {
                return detaching.Error;
            }

            await _repository.Update(parent.Value, ct);
        }
        await ArchiveLocationsOnlyOwnedByDepartment(department, ct);
        await ArchivePositionsOnlyOwnedByDepartment(department, ct);
        await _repository.ArchiveChildDepartments(department.Value, copied, ct);

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

    private async Task<Result<Department>> FindParent(Department department, CancellationToken ct)
    {
        if (department.Parent == null)
        {
            return department;
        }

        return await _repository.GetById(department.Parent.Value, useLock: true, ct);
    }

    private async Task ArchiveLocationsOnlyOwnedByDepartment(Department department, CancellationToken ct)
    {
        await _repository.DeleteSingleTimeAttachedDepartmentLocations(department, ct);
    }

    private async Task ArchivePositionsOnlyOwnedByDepartment(Department department, CancellationToken ct)
    {
        await _repository.DeleteSingleTimeAttachedDepartmentPositions(department, ct);
    }
}
