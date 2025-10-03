using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.PositionsContext;
using DirectoryService.Core.PositionsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Integrational.Tests;

public class UnitTest1 : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;
    private readonly IServiceProvider _services;

    public UnitTest1(TestApplicationFactory factory)
    {
        _factory = factory;
        _services = factory.Services;
    }

    [Fact]
    public async Task Simple_Database_Test()
    {
        PositionId id = new PositionId();
        PositionName name = PositionName.Create("position name");
        PositionDescription description = PositionDescription.Create("position description");
        EntityLifeCycle lifeCycle = new EntityLifeCycle();
        Position position = new Position(name, description, lifeCycle, id);

        await using (AsyncServiceScope writeScope = _services.CreateAsyncScope())
        {
            ServiceDbContext context =
                writeScope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            context.Positions.Add(position);
            await context.SaveChangesAsync();
        }

        await using (AsyncServiceScope readScope = _services.CreateAsyncScope())
        {
            ServiceDbContext context =
                readScope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            Position? added = await context.Positions.FirstOrDefaultAsync(p => p.Id == id);
            Assert.NotNull(added);
            Assert.Equal(added.Id, id);
            Assert.Equal(added.Name, name);
            Assert.Equal(added.Description, description);
            Assert.Equal(added.LifeCycle, lifeCycle);
        }
    }
}
