using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.PositionsContext.ValueObjects;
using DirectoryService.UseCases.Common.Extensions;
using FluentValidation;

namespace DirectoryService.UseCases.Positions.CreatePosition;

public sealed class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.Name).MustBeValid(PositionName.Create);
        RuleFor(x => x.Description).MustBeValid(PositionDescription.Create);
        RuleFor(x => x.DepartmentIdentifiers).MustBeValid(DepartmentsIdSet.Create);
    }
}