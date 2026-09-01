using DirectoryService.Core.PositionsContext;
using DirectoryService.Integrational.Tests.Positions;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests;

public sealed class TransactionRollbackTests
    : IClassFixture<TestApplicationFactory>,
        IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly PositionsTestsHelper _positions;

    public TransactionRollbackTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _positions = new PositionsTestsHelper(factory);
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabase();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Failed_Position_Creation_Leaves_No_Orphan_Row()
    {
        const string positionName = "Orphan Position";

        Result<Guid> createPosition = await _positions.CreateNewPosition(
            positionName,
            "Position bound to departments that do not exist",
            [Guid.NewGuid(), Guid.NewGuid()]
        );

        Assert.True(createPosition.IsFailure);

        IEnumerable<Position> found = await _positions.GetPositionsByName(positionName);
        Assert.Empty(found);
    }
}
