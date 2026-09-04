using BookCatalog.Application.Requests.User.Command;
using BookCatalog.Application.Services.User;
using FluentValidation;

namespace BookCatalog.Application.Validators.User.Command;

public class UpdateUserByIdCommandValidator : AbstractValidator<UpdateUserByIdCommand>
{
    private const string PhoneRegex = @"^\+?[0-9\s\-()]{7,20}$";
    private static readonly DateOnly MinBirthDate = new(1900, 1, 1);

    private readonly IUserService _userService;

    public UpdateUserByIdCommandValidator(IUserService userService)
    {
        _userService = userService;

        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("Id is required.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email is not a valid email address.")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters.")
            .MustAsync((cmd, email, ct) => _userService.EnsureEmailUniqueAsync(email, cmd.Id, ct))
            .WithMessage("Email is already taken.");

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(PhoneRegex)
            .WithMessage("Phone number is not in a valid format.")
            .MaximumLength(20)
            .WithMessage("Phone number must not exceed 20 characters.")
            .MustAsync((cmd, phone, ct) => _userService.EnsurePhoneNumberUniqueAsync(phone, cmd.Id, ct))
            .WithMessage("Phone number is already taken.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Birth date cannot be in the future.")
            .GreaterThan(MinBirthDate)
            .WithMessage("Birth date must be after 1900-01-01.");
    }
}
