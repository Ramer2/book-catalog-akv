using AutoMapper;
using BookCatalog.Application.Handlers.Loan.Query;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Loan.Query;
using BookCatalog.Application.Services.User;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
using BookCatalog.Domain.SearchModels;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class GetUserLoansQueryHandlerTests
{
    private Mock<ILoanRepository> _loanRepository = null!;
    private Mock<IUserRepository> _userRepository = null!;
    private IMapper _mapper = null!;
    private UserService _userService = null!;
    private GetUserLoansQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _loanRepository = new Mock<ILoanRepository>(MockBehavior.Strict);
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _userService = new UserService(_userRepository.Object);
        _sut = new GetUserLoansQueryHandler(_loanRepository.Object, _userService, _mapper);
    }

    [Test]
    public async Task Handle_Should_ReturnPagedUserLoans_WhenUserExists()
    {
        var user = UserFaker.User();
        var query = new GetUserLoansQuery
        {
            UserId = user.Id,
            Page = 1,
            PageSize = 10
        };

        var loan = LoanFaker.Loan(userId: user.Id);

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _loanRepository
            .Setup(r => r.GetLoansByUserIdAsync(
                user.Id,
                It.IsAny<LoanSearchModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<Loan>
            {
                Items = new List<Loan> { loan },
                TotalCount = 1,
                TotalPages = 1,
                Page = 1,
                PageSize = 10
            });

        var response = await _sut.Handle(query, CancellationToken.None);

        var items = response.Items.ToList();
        Assert.That(items, Has.Count.EqualTo(1),
            "Handler must forward every loan returned by the repository");
        Assert.That(items[0].UserId, Is.EqualTo(user.Id),
            "LoanResponse.UserId must match the requested user");
    }

    [Test]
    public async Task Handle_Should_ForwardIsReturnedAndPagingToRepository()
    {
        var user = UserFaker.User();
        var query = new GetUserLoansQuery
        {
            UserId = user.Id,
            IsReturned = false,
            SortBy = "BorrowedAt",
            Desc = true,
            Page = 2,
            PageSize = 5
        };

        LoanSearchModel? forwarded = null;

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _loanRepository
            .Setup(r => r.GetLoansByUserIdAsync(
                user.Id,
                It.IsAny<LoanSearchModel>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, LoanSearchModel, CancellationToken>((_, sm, _) => forwarded = sm)
            .ReturnsAsync(new BaseSearchModelPagedResponse<Loan>
            {
                Items = new List<Loan>(),
                TotalCount = 0,
                TotalPages = 0,
                Page = query.Page,
                PageSize = query.PageSize
            });

        await _sut.Handle(query, CancellationToken.None);

        Assert.That(forwarded, Is.Not.Null,
            "Handler must build and forward a LoanSearchModel to the repository");
        Assert.Multiple(() =>
        {
            Assert.That(forwarded!.UserId, Is.EqualTo(user.Id),
                "Forwarded search model must scope loans to the requested user");
            Assert.That(forwarded.IsReturned, Is.False,
                "Forwarded search model must preserve the IsReturned filter");
            Assert.That(forwarded.SortBy, Is.EqualTo(query.SortBy),
                "Forwarded search model must preserve SortBy");
            Assert.That(forwarded.Desc, Is.True,
                "Forwarded search model must preserve Desc");
            Assert.That(forwarded.Page, Is.EqualTo(query.Page),
                "Forwarded search model must preserve Page");
            Assert.That(forwarded.PageSize, Is.EqualTo(query.PageSize),
                "Forwarded search model must preserve PageSize");
        });
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_UserDoesNotExist()
    {
        var query = new GetUserLoansQuery { UserId = Guid.NewGuid(), Page = 1, PageSize = 10 };

        _userRepository
            .Setup(r => r.GetByIdAsync(query.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(query, CancellationToken.None),
            "GetUserLoansQueryHandler must throw EntityNotFoundException when the user does not exist");

        Assert.That(ex!.Message, Is.EqualTo("No user found for a given id."),
            "EntityNotFoundException should carry the user-id lookup message");

        _loanRepository.Verify(
            r => r.GetLoansByUserIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<LoanSearchModel>(),
                It.IsAny<CancellationToken>()),
            Times.Never,
            "Handler must NOT query loans if the user is missing");
    }
}
