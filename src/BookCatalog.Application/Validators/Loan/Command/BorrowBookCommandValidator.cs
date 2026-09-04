using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Loan;
using BookCatalog.Application.Requests.Loan.Command;
using FluentValidation;

namespace BookCatalog.Application.Validators.Loan.Command;

public class BorrowBookCommandValidator : AbstractValidator<BorrowBookCommand>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILoanService _loanService;

    public BorrowBookCommandValidator(
        IBookRepository bookRepository,
        IUserRepository userRepository,
        ILoanService loanService)
    {
        _bookRepository = bookRepository;
        _userRepository = userRepository;
        _loanService = loanService;

        RuleFor(x => x.BookId)
            .Cascade(CascadeMode.Stop)
            .NotEqual(Guid.Empty)
            .WithMessage("BookId is required.")
            .MustAsync(BookExistsAsync)
            .WithMessage("Book does not exist.")
            .MustAsync((bookId, ct) => _loanService.EnsureBookAvailableAsync(bookId, ct))
            .WithMessage("Book is currently borrowed and cannot be lent out.");

        RuleFor(x => x.UserId)
            .Cascade(CascadeMode.Stop)
            .NotEqual(Guid.Empty)
            .WithMessage("UserId is required.")
            .MustAsync(UserExistsAsync)
            .WithMessage("User does not exist.");
    }

    private async Task<bool> BookExistsAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        return book is not null;
    }

    private async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user is not null;
    }
}
