using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.UseCases.Departments.Contracts;
using Microsoft.EntityFrameworkCore;
using ResultLibrary;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Departments;

public sealed class DepartmentsRepository : IDepartmentsRepository
{
    private readonly ServiceDbContext _dbContext;

    public DepartmentsRepository(ServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Department>> GetById(Guid id, CancellationToken ct = default)
    {
        Result<DepartmentId> departmentId = DepartmentId.Create(id);
        return departmentId.IsFailure ? departmentId.Error : await GetById(departmentId, ct);
    }

    public async Task<Result<Department>> GetById(DepartmentId id, CancellationToken ct = default)
    {
        Department? department = await _dbContext.Departments
            .Include(d => d.Locations)
            .Include(d => d.Positions)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken: ct);
        
        return department == null
            ? Error.NotFoundError($"Подразделения с ID - {id.Value} не существует.")
            : department;
    }

    public async Task Add(Department department, CancellationToken ct = default) =>
        await _dbContext.Departments.AddAsync(department, ct);
}
