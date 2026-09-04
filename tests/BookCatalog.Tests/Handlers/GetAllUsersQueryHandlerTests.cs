using AutoMapper;
using BookCatalog.Application.Handlers.User.Query;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.User.Query;
using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class GetAllUsersQueryHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private IMapper _mapper = null!;
    private GetAllUsersQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _sut = new GetAllUsersQueryHandler(_userRepository.Object, _mapper);
    }

    [Test]
    public async Task Handle_Should_ReturnEmptyItems_When_RepositoryReturnsNoUsers()
    {
        var query = new GetAllUsersQuery { Page = 1, PageSize = 10 };
        _userRepository
            .Setup(r => r.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<User>
            {
                Items = new List<User>(),
                TotalCount = 0,
                TotalPages = 0,
                Page = query.Page,
                PageSize = query.PageSize
            });

        var response = await _sut.Handle(query, CancellationToken.None);

        Assert.That(response.Items, Is.Not.Null,
            "GetAllUsersResponse.Items must never be null so consumers can enumerate safely");
        Assert.That(response.Items, Is.Empty,
            "GetAllUsersResponse.Items must be empty when the repository has no users");
        Assert.Multiple(() =>
        {
            Assert.That(response.TotalCount, Is.EqualTo(0),
                "TotalCount must be zero when the repository has no matches");
            Assert.That(response.TotalPages, Is.EqualTo(0),
                "TotalPages must be zero when the repository has no matches");
            Assert.That(response.Page, Is.EqualTo(query.Page),
                "Response.Page must echo the page reported by the repository");
            Assert.That(response.PageSize, Is.EqualTo(query.PageSize),
                "Response.PageSize must echo the page size reported by the repository");
        });
    }

    [Test]
    public async Task Handle_Should_ReturnMappedUsersInSameOrder_When_RepositoryReturnsMultiple()
    {
        var first = UserFaker.User(email: "a@example.com");
        var second = UserFaker.User(email: "b@example.com");
        var third = UserFaker.User(email: "c@example.com");
        var query = new GetAllUsersQuery { Page = 1, PageSize = 10 };

        _userRepository
            .Setup(r => r.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<User>
            {
                Items = new List<User> { first, second, third },
                TotalCount = 3,
                TotalPages = 1,
                Page = query.Page,
                PageSize = query.PageSize
            });

        var response = await _sut.Handle(query, CancellationToken.None);

        var items = response.Items.ToList();
        Assert.That(items, Has.Count.EqualTo(3),
            "GetAllUsersResponse.Items must contain one entry per repository user");
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Email, Is.EqualTo(first.Email),
                "First UserResponse must correspond to the first user returned by the repository");
            Assert.That(items[1].Email, Is.EqualTo(second.Email),
                "Second UserResponse must correspond to the second user returned by the repository");
            Assert.That(items[2].Email, Is.EqualTo(third.Email),
                "Third UserResponse must correspond to the third user returned by the repository");
        });
    }

    [Test]
    public async Task Handle_Should_ForwardQueryToRepository_When_FiltersAndSortAreProvided()
    {
        var query = new GetAllUsersQuery
        {
            Email = "example",
            FirstName = "John",
            SortBy = "Email",
            Desc = true,
            Page = 2,
            PageSize = 5
        };

        _userRepository
            .Setup(r => r.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<User>
            {
                Items = new List<User>(),
                TotalCount = 0,
                TotalPages = 0,
                Page = query.Page,
                PageSize = query.PageSize
            });

        await _sut.Handle(query, CancellationToken.None);

        _userRepository.Verify(
            r => r.GetAllAsync(query, It.IsAny<CancellationToken>()),
            Times.Once,
            "Handler must forward the incoming query (filters, sort and paging) to the repository unchanged");
    }
}
