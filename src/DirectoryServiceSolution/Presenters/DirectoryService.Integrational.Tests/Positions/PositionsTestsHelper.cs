using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.Core.PositionsContext;
using DirectoryService.UseCases.Departments.Contracts;
using DirectoryService.UseCases.Positions.Contracts;
using DirectoryService.UseCases.Positions.CreatePosition;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Positions;

public sealed class PositionsTestsHelper
{
    private readonly IServiceProvider _services;

    public PositionsTestsHelper(TestApplicationFactory factory)
    {
        _services = factory.Services;
    }


    public async Task<Result<Guid>> CreateNewPosition(
        string name,
        string description,
        IEnumerable<Guid> departmentIds
    )
    {
        CreatePositionCommand command = new(name, description, departmentIds);
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ICommandHandler<Guid, CreatePositionCommand> handler = scope.GetService<
            ICommandHandler<Guid, CreatePositionCommand>
        >();
        return await handler.Handle(command);
    }

    public async Task<IEnumerable<DepartmentPosition>> GetDepartmentPositions(Guid positionId)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        IDepartmentPositionsRepository repository =
            scope.GetService<IDepartmentPositionsRepository>();

        return await repository.Get(
            new DepartmentPositionSpecification().WithPositionId(positionId)
        );
    }

    public async Task<IEnumerable<Position>> GetPositionsByName(string name)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        IPositionsRepository repository = scope.GetService<IPositionsRepository>();
        return await repository.Get(new PositionSpecification().WithName(name));
    }
}
