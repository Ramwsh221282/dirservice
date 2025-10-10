using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Departments;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Departments.Contracts;
using DirectoryService.UseCases.Departments.CreateDepartment;
using DirectoryService.UseCases.Departments.MoveDepartment;
using DirectoryService.WebApi;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Departments;

public sealed class DepartmentsTestsHelper
{
    private readonly IServiceProvider _services;

    public DepartmentsTestsHelper(TestApplicationFactory factory)
    {
        _services = factory.Services;
    }

    public DepartmentsTestsHelper(WebApplicationFactory<Program> factory)
    {
        _services = factory.Services;
    }

    public async Task<Result<Guid>> CreateNewDepartment(
        string name,
        string identifier,
        IEnumerable<Guid> locationIds,
        Guid? parentId = null
    )
    {
        CreateDepartmentCommand createDepartment = new(name, identifier, locationIds, parentId);
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ICommandHandler<Guid, CreateDepartmentCommand> createDepartmentHandler = scope.GetService<
            ICommandHandler<Guid, CreateDepartmentCommand>
        >();
        return await createDepartmentHandler.Handle(createDepartment);
    }

    public async Task<Result<Guid>> MoveDepartment(Guid parentId, Guid movableId)
    {
        MoveDepartmentCommand createDepartment = new(parentId, movableId);
        await using var scope = _services.CreateAsyncScope();
        ICommandHandler<Guid, MoveDepartmentCommand> moveHandler = scope.GetService<
            ICommandHandler<Guid, MoveDepartmentCommand>
        >();
        return await moveHandler.Handle(createDepartment);
    }

    public async Task<Result<Department>> GetDepartment(Guid id)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
        IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
        Result<Department> department = await repository.GetById(id);
        return department;
    }
}
