using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Common.Extensions;
using FluentValidation;

namespace DirectoryService.UseCases.Departments.UpdateDepartmentLocations;

public sealed class UpdateDepartmentLocationsCommandValidator
    : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsCommandValidator()
    {
        RuleFor(v => v.DepartmentId).MustBeValid(DepartmentId.Create);
        RuleFor(v => v.LocationIds).MustBeValid(LocationsIdSet.Create);
    }
}
