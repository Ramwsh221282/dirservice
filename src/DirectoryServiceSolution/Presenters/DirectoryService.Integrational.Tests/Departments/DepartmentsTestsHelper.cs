using System.Data;
using Dapper;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Departments.Contracts;
using DirectoryService.UseCases.Departments.CreateDepartment;
using DirectoryService.UseCases.Departments.DeleteDepartment;
using DirectoryService.UseCases.Departments.MoveDepartment;
using DirectoryService.UseCases.Departments.UpdateDepartmentLocations;
using DirectoryService.WebApi.DependencyInjection;
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
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ICommandHandler<Guid, MoveDepartmentCommand> moveHandler = scope.GetService<
            ICommandHandler<Guid, MoveDepartmentCommand>
        >();
        return await moveHandler.Handle(createDepartment);
    }

    public async Task<Result<Department>> GetDepartment(Guid id)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        IDepartmentsRepository repository = scope.GetService<IDepartmentsRepository>();
        Result<Department> department = await repository.GetById(id);
        return department;
    }

    public async Task<Result<Guid>> UpdateDepartmentLocations(
        Guid departmentId,
        IEnumerable<Guid> locationIds
    )
    {
        UpdateDepartmentLocationsCommand command = new(departmentId, locationIds);
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ICommandHandler<Guid, UpdateDepartmentLocationsCommand> handler = scope.GetService<
            ICommandHandler<Guid, UpdateDepartmentLocationsCommand>
        >();
        return await handler.Handle(command);
    }

    public async Task<Result<Guid>> DeleteDepartment(Guid id)
    {
        DeleteDepartmentCommand command = new(id);
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ICommandHandler<Guid, DeleteDepartmentCommand> handler = scope.GetService<
            ICommandHandler<Guid, DeleteDepartmentCommand>
        >();
        return await handler.Handle(command);
    }

    public async Task<IEnumerable<DepartmentLocation>> GetDepartmentLocations(Guid departmentId)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        IDepartmentLocationsRepository repository =
            scope.GetService<IDepartmentLocationsRepository>();

        return await repository.Get(
            new DepartmentLocationSpecification().WithDepartmentId(departmentId)
        );
    }

    public async Task<int> GetChildrensCountRaw(Guid departmentId)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        IDbConnection connection = scope.GetService<IDbConnection>();

        CommandDefinition command = new(
            "SELECT childrens_count FROM departments WHERE id = @Id",
            new { Id = departmentId }
        );

        return await connection.ExecuteScalarAsync<int>(command);
    }
}
