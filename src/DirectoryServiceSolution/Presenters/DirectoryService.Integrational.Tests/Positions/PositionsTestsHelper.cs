using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Positions.CreatePosition;
using DirectoryService.WebApi;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
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

    public PositionsTestsHelper(WebApplicationFactory<Program> factory)
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
        await using var scope = _services.CreateAsyncScope();
        var handler = scope.GetService<
            ICommandHandler<Guid, CreatePositionCommand>
        >();
        return await handler.Handle(command);
    }
}