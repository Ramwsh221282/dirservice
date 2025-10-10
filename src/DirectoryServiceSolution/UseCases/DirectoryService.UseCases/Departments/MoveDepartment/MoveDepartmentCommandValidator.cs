using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.UseCases.Common.Extensions;
using FluentValidation;

namespace DirectoryService.UseCases.Departments.MoveDepartment;

public sealed class MoveDepartmentCommandValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentCommandValidator()
    {
        RuleFor(x => x.MovableId).MustBeValid(DepartmentId.Create);
        RuleFor(x => x.MoveToId).MustBeValid(DepartmentId.Create);
    }
}
