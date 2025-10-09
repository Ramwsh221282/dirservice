using DirectoryService.Contracts.Departments;
using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Departments.UpdateDepartmentLocations;

public sealed record UpdateDepartmentLocationsCommand : ICommand<Guid>
{
    public Guid DepartmentId { get; }
    public IEnumerable<Guid> LocationIds { get; }

    public UpdateDepartmentLocationsCommand(Guid departmentId, IEnumerable<Guid> locationIds)
    {
        DepartmentId = departmentId;
        LocationIds = locationIds;
    }

    public UpdateDepartmentLocationsCommand(UpdateDepartmentLocationsRequest request)
        : this(request.DepartmentId, request.LocationIds) { }
}
