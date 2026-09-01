using DirectoryService.Core.PositionsContext;
using DirectoryService.Core.PositionsContext.ValueObjects;

namespace DirectoryService.UseCases.Positions.Contracts;

public interface IPositionsRepository
{
    Task Add(Position position, CancellationToken ct = default);
    Task<PositionNameUniquesness> IsUnique(PositionName name, CancellationToken ct = default);
    Task Update(Position position, CancellationToken ct = default);

    Task<IEnumerable<Position>> Get(
        PositionSpecification specification,
        CancellationToken ct = default
    );
}
