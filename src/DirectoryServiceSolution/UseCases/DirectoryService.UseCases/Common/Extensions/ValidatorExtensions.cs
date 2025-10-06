using FluentValidation;
using FluentValidation.Results;
using ResultLibrary;

namespace DirectoryService.UseCases.Common.Extensions;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptionsConditions<T, TU> MustBeValid<T, TU>(
        this IRuleBuilderInitial<T, TU> rule,
        Func<TU, Result> resultFactory
    )
    {
        return rule.Custom(
            (property, context) =>
            {
                Result result = resultFactory(property);
                if (result.IsFailure)
                {
                    ValidationFailure failure = new ValidationFailure()
                    {
                        ErrorMessage = result.Error.Message,
                    };
                    context.AddFailure(failure);
                }
            }
        );
    }

    public static IRuleBuilderOptionsConditions<T, IEnumerable<TU>> AllMustBeValid<T, TU>(
        this IRuleBuilderInitial<T, IEnumerable<TU>> rule,
        Func<TU, Result> resultFactory
    )
    {
        return rule.Custom(
            (properties, context) =>
            {
                foreach (var property in properties)
                {
                    Result result = resultFactory(property);
                    if (result.IsFailure)
                    {
                        ValidationFailure failure = new ValidationFailure()
                        {
                            ErrorMessage = result.Error.Message,
                        };
                        context.AddFailure(failure);
                    }
                }
            }
        );
    }
}
