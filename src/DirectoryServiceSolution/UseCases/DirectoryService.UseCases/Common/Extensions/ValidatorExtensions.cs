using FluentValidation;
using FluentValidation.Results;
using ResultLibrary;

namespace DirectoryService.UseCases.Common.Extensions;

public static class ValidatorExtensions
{
    public static Result<T> AsFailureResult<T>(this ValidationResult result)
    {
        List<ValidationFailure> failures = result.Errors;
        if (failures.Count == 1)
            return failures[0].ErrorFromValidationFailure();

        ErrorsCollection collection = new ErrorsCollection();
        foreach (ValidationFailure failure in failures)
            collection.Add(failure.ErrorFromValidationFailure());
        return Result<T>.Fail(collection);
    }

    public static Result AsFailureResult(this ValidationResult result)
    {
        List<ValidationFailure> failures = result.Errors;
        if (failures.Count == 1)
            return failures[0].ErrorFromValidationFailure();

        ErrorsCollection collection = new ErrorsCollection();
        foreach (ValidationFailure failure in failures)
            collection.Add(failure.ErrorFromValidationFailure());
        return collection;
    }

    public static IRuleBuilderOptionsConditions<T, TU> MustBeValid<T, TU>(
        this IRuleBuilderInitial<T, TU> rule,
        Func<TU, Result> resultFactory
    )
    {
        return rule.Custom((property, context) => property.ManageResult(resultFactory, context));
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
                    property.ManageResult(resultFactory, context);
            }
        );
    }

    private static Error ErrorFromValidationFailure(this ValidationFailure failure)
    {
        string code = failure.ErrorCode;
        string message = failure.ErrorMessage;
        ErrorType type = code.DispatchErrorTypeByCode();
        Error error = new Error(message, type);
        return error;
    }

    private static ErrorType DispatchErrorTypeByCode(this string code) =>
        code switch
        {
            nameof(ConflictErrorType) => new ConflictErrorType(),
            nameof(ExceptionalErrorType) => new ExceptionalErrorType(),
            nameof(NotFoundErrorType) => new NotFoundErrorType(),
            nameof(ValidationErrorType) => new ValidationErrorType(),
            _ => throw new ApplicationException(
                "Код ошибки либо не содержит ошибку, либо не поддерживается."
            ),
        };

    private static void ManageResult<T, TU>(
        this TU property,
        Func<TU, Result> resultFactory,
        ValidationContext<T> context
    )
    {
        Result result = resultFactory(property);
        if (!result.IsFailure)
            return;
        ValidationFailure failure = result.Error.ValidationFailureFromError();
        context.AddFailure(failure);
    }

    private static ValidationFailure ValidationFailureFromError(this Error error)
    {
        return new ValidationFailure()
        {
            ErrorMessage = error.Message,
            ErrorCode = error.Type.GetType().Name,
        };
    }
}
