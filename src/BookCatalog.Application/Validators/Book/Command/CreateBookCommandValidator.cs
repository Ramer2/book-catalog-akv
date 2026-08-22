using BookCatalog.Application.Requests.Book.Command;
using FluentValidation;

namespace BookCatalog.Application.Validators.Book.Command;

public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Isbn)
            .NotEmpty()
            .WithMessage("ISBN is required.")
            .Length(10, 13)
            .WithMessage("ISBN must be between 10 and 13 characters long.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(256)
            .WithMessage("Title must not exceed 256 characters.");

        RuleFor(x => x.Author)
            .NotEmpty()
            .WithMessage("Author is required.")
            .MaximumLength(256)
            .WithMessage("Author must not exceed 256 characters.");

        RuleFor(x => x.NumberOfPages)
            .GreaterThan(0)
            .WithMessage("Number of pages must be greater than 0.");

        RuleFor(x => x.PublishDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Publish date cannot be in the future.")
            .When(x => x.PublishDate.HasValue);
    }
}
