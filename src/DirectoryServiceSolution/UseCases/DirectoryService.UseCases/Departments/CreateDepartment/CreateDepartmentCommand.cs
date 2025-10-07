using DirectoryService.Contracts.Departments;
using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Departments.CreateDepartment;

public sealed record CreateDepartmentCommand : ICommand<Guid>
{
    public string Name { get; }
    public Guid? ParentId { get; }
    public string Identifier { get; }
    public IReadOnlyList<Guid> LocationIds { get; }

    public CreateDepartmentCommand(CreateDepartmentRequest request)
        : this(
            request.Name.Value,
            request.Identifier.Value,
            request.Locations.Select(l => l.Value),
            request.ParentId?.Value
        ) { }

    public CreateDepartmentCommand(
        string name,
        string identifier,
        IEnumerable<Guid> locationIds,
        Guid? parentId = null
    )
    {
        Name = name;
        Identifier = identifier;
        LocationIds = [.. locationIds];
        ParentId = parentId;
    }
}
