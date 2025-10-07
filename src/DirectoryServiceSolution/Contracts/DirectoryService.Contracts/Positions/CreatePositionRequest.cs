namespace DirectoryService.Contracts.Positions;

public sealed record CreatePositionRequest(
    PositionNameDto Name,
    PositionDescriptionDto Description,
    PositionDepartmentIdentifiersDto Identifiers);