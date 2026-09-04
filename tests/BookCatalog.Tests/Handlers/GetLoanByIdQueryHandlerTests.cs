using AutoMapper;
using BookCatalog.Application.Handlers.Loan.Query;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Loan.Query;
using BookCatalog.Application.Services.Loan;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class GetLoanByIdQueryHandlerTests
{
    private Mock<ILoanRepository> _loanRepository = null!;
    private IMapper _mapper = null!;
    private LoanService _loanService = null!;
    private GetLoanByIdQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _loanRepository = new Mock<ILoanRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _loanService = new LoanService(_loanRepository.Object);
        _sut = new GetLoanByIdQueryHandler(_mapper, _loanService);
    }

    [Test]
    public async Task Handle_Should_ReturnMappedResponse_When_LoanExists()
    {
        var existing = LoanFaker.Loan();
        _loanRepository
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var response = await _sut.Handle(new GetLoanByIdQuery { Id = existing.Id }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(existing.Id), "LoanResponse.Id must match the fetched loan");
            Assert.That(response.BookId, Is.EqualTo(existing.BookId),
                "LoanResponse.BookId must match the fetched loan");
            Assert.That(response.UserId, Is.EqualTo(existing.UserId),
                "LoanResponse.UserId must match the fetched loan");
            Assert.That(response.BorrowedAt, Is.EqualTo(existing.BorrowedAt),
                "LoanResponse.BorrowedAt must match the fetched loan");
            Assert.That(response.ReturnedAt, Is.EqualTo(existing.ReturnedAt),
                "LoanResponse.ReturnedAt must match the fetched loan");
        });
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_LoanDoesNotExist()
    {
        var missingId = Guid.NewGuid();
        _loanRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(new GetLoanByIdQuery { Id = missingId }, CancellationToken.None),
            "GetLoanByIdQueryHandler must throw EntityNotFoundException when the loan cannot be located");

        Assert.That(ex!.Message, Is.EqualTo("No loan found for a given id."),
            "EntityNotFoundException should carry the loan-id lookup message");
    }
}
