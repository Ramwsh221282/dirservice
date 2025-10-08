namespace DirectoryService.Contracts.Positions;

public sealed record CreatePositionRequest(
    string Name,
    string Description,
    PositionDepartmentIdentifiersDto Identifiers
);
