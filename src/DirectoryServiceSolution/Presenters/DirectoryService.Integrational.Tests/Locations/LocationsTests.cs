using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Locations.CreateLocation;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Locations;

public class LocationsTests : IClassFixture<TestApplicationFactory>
{
    private readonly IServiceProvider _services;

    public LocationsTests(TestApplicationFactory factory)
    {
        _services = factory.Services;
    }

    [Fact]
    public async Task Create_Location_Name_Duplicate_Failure()
    {
        LocationName name = LocationName.Create("Test Location");
        LocationTimeZone timeZone = LocationTimeZone.Create("Big/City");
        IEnumerable<string> addressParts = ["Some", "Big", "City"];

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            CreateLocationCommand command = new(name.Value, addressParts, timeZone.Value);
            ICommandHandler<Guid, CreateLocationCommand> handler = scope.GetService<
                ICommandHandler<Guid, CreateLocationCommand>
            >();
            Result<Guid> result = await handler.Handle(command);
            Assert.True(result.IsSuccess);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext context = scope.GetService<ServiceDbContext>();
            Location? location = await context.Locations.FirstOrDefaultAsync(l =>
                l.Name == LocationName.Create(name.Value)
            );
            Assert.NotNull(location);
            Assert.Equal(location.Name.Value, name.Value);
            Assert.Equal(location.TimeZone.Value, timeZone.Value);
            Assert.Contains(location.Address.Parts, p => addressParts.Any(ap => ap.Equals(p.Node)));
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            CreateLocationCommand command = new(name.Value, addressParts, timeZone.Value);
            ICommandHandler<Guid, CreateLocationCommand> handler = scope.GetService<
                ICommandHandler<Guid, CreateLocationCommand>
            >();
            Result<Guid> result = await handler.Handle(command);
            Assert.True(result.IsFailure);
        }
    }

    [Fact]
    public async Task Create_Location_Success()
    {
        LocationName name = LocationName.Create("Test Location");
        LocationTimeZone timeZone = LocationTimeZone.Create("Big/City");
        IEnumerable<string> addressParts = ["Some", "Big", "City"];

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            CreateLocationCommand command = new(name.Value, addressParts, timeZone.Value);
            ICommandHandler<Guid, CreateLocationCommand> handler = scope.GetService<
                ICommandHandler<Guid, CreateLocationCommand>
            >();
            Result<Guid> result = await handler.Handle(command);
            Assert.True(result.IsSuccess);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext context = scope.GetService<ServiceDbContext>();
            Location? location = await context.Locations.FirstOrDefaultAsync(l =>
                l.Name == LocationName.Create(name.Value)
            );
            Assert.NotNull(location);
            Assert.Equal(location.Name.Value, name.Value);
            Assert.Equal(location.TimeZone.Value, timeZone.Value);
            Assert.Contains(location.Address.Parts, p => addressParts.Any(ap => ap.Equals(p.Node)));
        }
    }

    [Fact]
    public async Task Create_Location_Name_Failure()
    {
        string name = "   ";
        IEnumerable<string> addressParts = ["Some", "Big", "City"];
        string timeZone = "Big/City";

        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        CreateLocationCommand command = new(name, addressParts, timeZone);
        ICommandHandler<Guid, CreateLocationCommand> handler = scope.GetService<
            ICommandHandler<Guid, CreateLocationCommand>
        >();
        Result<Guid> result = await handler.Handle(command);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Create_Location_TimeZone_Failure()
    {
        string name = "Test Location";
        IEnumerable<string> addressParts = ["Some", "Big", "City"];
        string timeZone = "Some Random String";

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            CreateLocationCommand command = new(name, addressParts, timeZone);
            ICommandHandler<Guid, CreateLocationCommand> handler = scope.GetService<
                ICommandHandler<Guid, CreateLocationCommand>
            >();
            Result<Guid> result = await handler.Handle(command);
            Assert.True(result.IsFailure);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext context = scope.GetService<ServiceDbContext>();
            Location? location = await context.Locations.FirstOrDefaultAsync(l =>
                l.Name == LocationName.Create(name)
            );
            Assert.Null(location);
        }
    }

    [Fact]
    public async Task Create_Location_Address_Failure()
    {
        string name = "Test Location";
        IEnumerable<string> addressParts = [];
        string timeZone = "Big/City";

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            CreateLocationCommand command = new(name, addressParts, timeZone);
            ICommandHandler<Guid, CreateLocationCommand> handler = scope.GetService<
                ICommandHandler<Guid, CreateLocationCommand>
            >();
            Result<Guid> result = await handler.Handle(command);
            Assert.True(result.IsFailure);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext context = scope.GetService<ServiceDbContext>();
            Location? location = await context.Locations.FirstOrDefaultAsync(l =>
                l.Name == LocationName.Create(name)
            );
            Assert.Null(location);
        }
    }
}
