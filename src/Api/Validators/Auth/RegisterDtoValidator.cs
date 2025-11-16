using Api.DTOs.Account;
using FluentValidation;

namespace Api.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
            .MaximumLength(32).WithMessage("Username must be no more than 32 characters long");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters long")
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase")
            .Matches("[a-z]").WithMessage("Password must contain at least 1 lowercase")
            .Matches("[0-9]").WithMessage("Password must contain a digit");
    }
}