using AutoMapper;
using BookCatalog.Application.Handlers.Loan.Query;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Loan.Query;
using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class GetAllLoansQueryHandlerTests
{
    private Mock<ILoanRepository> _loanRepository = null!;
    private IMapper _mapper = null!;
    private GetAllLoansQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _loanRepository = new Mock<ILoanRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _sut = new GetAllLoansQueryHandler(_loanRepository.Object, _mapper);
    }

    [Test]
    public async Task Handle_Should_ReturnEmptyItems_When_RepositoryReturnsNoLoans()
    {
        var query = new GetAllLoansQuery { Page = 1, PageSize = 10 };
        _loanRepository
            .Setup(r => r.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<Loan>
            {
                Items = new List<Loan>(),
                TotalCount = 0,
                TotalPages = 0,
                Page = query.Page,
                PageSize = query.PageSize
            });

        var response = await _sut.Handle(query, CancellationToken.None);

        Assert.That(response.Items, Is.Empty,
            "GetAllLoansResponse.Items must be empty when the repository has no loans");
        Assert.Multiple(() =>
        {
            Assert.That(response.TotalCount, Is.EqualTo(0),
                "TotalCount must be zero when the repository has no matches");
            Assert.That(response.Page, Is.EqualTo(query.Page),
                "Response.Page must echo the page reported by the repository");
            Assert.That(response.PageSize, Is.EqualTo(query.PageSize),
                "Response.PageSize must echo the page size reported by the repository");
        });
    }

    [Test]
    public async Task Handle_Should_ReturnMappedLoans_When_RepositoryReturnsMultiple()
    {
        var first = LoanFaker.Loan();
        var second = LoanFaker.Loan();
        var query = new GetAllLoansQuery { Page = 1, PageSize = 10 };

        _loanRepository
            .Setup(r => r.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<Loan>
            {
                Items = new List<Loan> { first, second },
                TotalCount = 2,
                TotalPages = 1,
                Page = query.Page,
                PageSize = query.PageSize
            });

        var response = await _sut.Handle(query, CancellationToken.None);

        var items = response.Items.ToList();
        Assert.That(items, Has.Count.EqualTo(2),
            "GetAllLoansResponse.Items must contain one entry per repository loan");
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Id, Is.EqualTo(first.Id),
                "First LoanResponse must correspond to the first loan returned by the repository");
            Assert.That(items[1].Id, Is.EqualTo(second.Id),
                "Second LoanResponse must correspond to the second loan returned by the repository");
        });
    }

    [Test]
    public async Task Handle_Should_ForwardQueryToRepository_When_FiltersProvided()
    {
        var query = new GetAllLoansQuery
        {
            BookId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsReturned = false,
            SortBy = "BorrowedAt",
            Desc = true,
            Page = 2,
            PageSize = 5
        };

        _loanRepository
            .Setup(r => r.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<Loan>
            {
                Items = new List<Loan>(),
                TotalCount = 0,
                TotalPages = 0,
                Page = query.Page,
                PageSize = query.PageSize
            });

        await _sut.Handle(query, CancellationToken.None);

        _loanRepository.Verify(
            r => r.GetAllAsync(query, It.IsAny<CancellationToken>()),
            Times.Once,
            "Handler must forward the incoming query (filters, sort and paging) to the repository unchanged");
    }
}
