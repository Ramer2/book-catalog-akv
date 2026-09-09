using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Application.Services.Author;
using BookCatalog.Application.Services.Isbn;
using BookCatalog.Domain.Exceptions;
using FluentValidation;

namespace BookCatalog.Application.Validators.Book.Command;

public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    private readonly IIsbnService _isbnService;
    private readonly IAuthorService _authorService;

    public CreateBookCommandValidator(IIsbnService isbnService, IAuthorService authorService)
    {
        _isbnService = isbnService;
        _authorService = authorService;

        RuleFor(x => x.Isbn)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("ISBN is required.")
            .Length(10, 13)
            .WithMessage("ISBN must be between 10 and 13 characters long.")
            .MustAsync((isbn, ct) => _isbnService.EnsureIsbnUniqueAsync(isbn, null, ct))
            .WithMessage("ISBN is already taken.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(256)
            .WithMessage("Title must not exceed 256 characters.");

        RuleFor(x => x.AuthorId)
            .Cascade(CascadeMode.Stop)
            .NotEqual(Guid.Empty)
            .WithMessage("AuthorId is required.")
            .MustAsync(AuthorExistsAsync)
            .WithMessage("Author not found.");

        RuleFor(x => x.NumberOfPages)
            .GreaterThan(0)
            .WithMessage("Number of pages must be greater than 0.");

        RuleFor(x => x.PublishDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Publish date cannot be in the future.")
            .When(x => x.PublishDate.HasValue);
    }

    private async Task<bool> AuthorExistsAsync(Guid authorId, CancellationToken cancellationToken)
    {
        try
        {
            await _authorService.GetOrThrowAsync(authorId, cancellationToken);
            return true;
        }
        catch (EntityNotFoundException)
        {
            return false;
        }
    }
}
