using FluentValidation;

namespace DirectoryService.Infrastructure.Identity.Commands.SignUp;

public sealed class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    private const int MinLoginLength = 3;
    private const int MinPasswordLength = 6;

    public SignUpCommandValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty()
            .WithMessage("Логин не может быть пустым.")
            .MinimumLength(MinLoginLength)
            .WithMessage($"Логин должен содержать минимум {MinLoginLength} символа.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Пароль не может быть пустым.")
            .MinimumLength(MinPasswordLength)
            .WithMessage($"Пароль должен содержать минимум {MinPasswordLength} символов.");
    }
}
