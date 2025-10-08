using DirectoryService.Contracts.Positions;
using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Positions.CreatePosition;

public sealed record CreatePositionCommand : ICommand<Guid>
{
    public string Name { get; }
    public IEnumerable<Guid> DepartmentIdentifiers { get; }
    public string Description { get; }

    public CreatePositionCommand(
        string name,
        string description,
        IEnumerable<Guid> departmentIdentifiers
    )
    {
        Name = name;
        Description = description;
        DepartmentIdentifiers = departmentIdentifiers;
    }

    public CreatePositionCommand(CreatePositionRequest request)
        : this(
            request.Name,
            request.Description,
            request.Identifiers.DepartmentIdentifiers.Select(i => i)
        ) { }
}
