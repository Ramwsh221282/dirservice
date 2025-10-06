using DirectoryService.Contracts;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.UseCases.Locations.CreateLocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests;

public class LocationsTests : IClassFixture<TestApplicationFactory>
{
    private readonly IServiceProvider _services;

    public LocationsTests(TestApplicationFactory factory)
    {
        _services = factory.Services;
    }

    [Fact]
    public async Task Create_Location_Success()
    {
        string name = "Test Location";
        IEnumerable<string> addressParts = ["Some", "Big", "City"];
        string timeZone = "Big/City";

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            CreateLocationCommand command = new(name, addressParts, timeZone);
            CreateLocationCommandHandler handler =
                scope.ServiceProvider.GetRequiredService<CreateLocationCommandHandler>();
            Result<Guid> result = await handler.Handle(command);
            Assert.True(result.IsSuccess);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext context =
                scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            Location? location = await context.Locations.FirstOrDefaultAsync(l =>
                l.Name == LocationName.Create(name).Value
            );
            Assert.NotNull(location);
            Assert.Equal(location.Name.Value, name);
            Assert.Equal(location.TimeZone.Value, timeZone);
            Assert.Contains(location.Address.Parts, p => addressParts.Any(ap => ap.Equals(p.Node)));
        }
    }

    [Fact]
    public async Task Create_Location_Name_Failure()
    {
        string name = "   ";
        IEnumerable<string> addressParts = ["Some", "Big", "City"];
        string timeZone = "Big/City";

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            CreateLocationCommand command = new(name, addressParts, timeZone);
            CreateLocationCommandHandler handler =
                scope.ServiceProvider.GetRequiredService<CreateLocationCommandHandler>();
            Result<Guid> result = await handler.Handle(command);
            Assert.True(result.IsFailure);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext context =
                scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            Location? location = await context.Locations.FirstOrDefaultAsync(l =>
                l.Name == LocationName.Create(name)
            );
            Assert.Null(location);
        }
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
            CreateLocationCommandHandler handler =
                scope.ServiceProvider.GetRequiredService<CreateLocationCommandHandler>();
            Result<Guid> result = await handler.Handle(command);
            Assert.True(result.IsFailure);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext context =
                scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
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
            CreateLocationCommandHandler handler =
                scope.ServiceProvider.GetRequiredService<CreateLocationCommandHandler>();
            Result<Guid> result = await handler.Handle(command);
            Assert.True(result.IsFailure);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext context =
                scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            Location? location = await context.Locations.FirstOrDefaultAsync(l =>
                l.Name == LocationName.Create(name)
            );
            Assert.Null(location);
        }
    }
}
