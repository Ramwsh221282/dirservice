using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Common.Extensions;
using FluentValidation;

namespace DirectoryService.UseCases.Locations.CreateLocation;

public sealed class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Name).MustBeValid(LocationName.Create);
        RuleFor(x => x.TimeZone).MustBeValid(LocationTimeZone.Create);
        RuleFor(x => x.AddressParts).AllMustBeValid(LocationAddressPart.Create);
    }
}
