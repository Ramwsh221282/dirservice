using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Locations.Contracts;
using DirectoryService.UseCases.Locations.CreateLocation;
using DirectoryService.WebApi;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Locations;

public sealed class LocationsTestsHelper
{
    private readonly IServiceProvider _services;

    public LocationsTestsHelper(TestApplicationFactory factory)
    {
        _services = factory.Services;
    }

    public LocationsTestsHelper(WebApplicationFactory<Program> factory)
    {
        _services = factory.Services;
    }

    public async Task<Result<Guid>> CreateNewLocation(
        string name,
        string timeZone,
        IEnumerable<string> addressParts
    )
    {
        CreateLocationCommand command = new(name, addressParts, timeZone);
        await using var scope = _services.CreateAsyncScope();
        var createLocationHandler = scope.GetService<
            ICommandHandler<Guid, CreateLocationCommand>
        >();
        return await createLocationHandler.Handle(command);
    }

    public async Task<Result<Location>> GetLocation(Guid locationId)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ILocationsRepository repository = scope.GetService<ILocationsRepository>();
        return await repository.GetById(locationId);
    }
}