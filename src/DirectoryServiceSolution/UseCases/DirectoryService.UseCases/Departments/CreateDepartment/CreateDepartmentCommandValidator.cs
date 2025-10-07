using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Common.Extensions;
using FluentValidation;

namespace DirectoryService.UseCases.Departments.CreateDepartment;

public sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(c => c.Name).MustBeValid(DepartmentName.Create);
        RuleFor(c => c.Identifier).MustBeValid(DepartmentIdentifier.Create);
        RuleFor(c => c.LocationIds).MustBeValid(LocationsIdSet.Create);
    }
}
