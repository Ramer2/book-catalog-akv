using AutoMapper;
using BookCatalog.Application.Handlers.User.Query;
using BookCatalog.Application.Requests.User.Query;
using BookCatalog.Application.Services.User;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class GetUserByIdQueryHandlerTests
{
    private Mock<IUserService> _userService = null!;
    private IMapper _mapper = null!;
    private GetUserByIdQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userService = new Mock<IUserService>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _sut = new GetUserByIdQueryHandler(_mapper, _userService.Object);
    }

    [Test]
    public async Task Handle_Should_ReturnMappedResponse_When_UserExists()
    {
        var existing = UserFaker.User();
        _userService
            .Setup(s => s.GetOrThrowAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var response = await _sut.Handle(new GetUserByIdQuery { Id = existing.Id }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(existing.Id), "UserResponse.Id must match the fetched user");
            Assert.That(response.Email, Is.EqualTo(existing.Email), "UserResponse.Email must match the fetched user");
            Assert.That(response.PhoneNumber, Is.EqualTo(existing.PhoneNumber),
                "UserResponse.PhoneNumber must match the fetched user");
            Assert.That(response.FirstName, Is.EqualTo(existing.FirstName),
                "UserResponse.FirstName must match the fetched user");
            Assert.That(response.LastName, Is.EqualTo(existing.LastName),
                "UserResponse.LastName must match the fetched user");
            Assert.That(response.BirthDate, Is.EqualTo(existing.BirthDate),
                "UserResponse.BirthDate must match the fetched user");
        });
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_UserDoesNotExist()
    {
        var missingId = Guid.NewGuid();
        _userService
            .Setup(s => s.GetOrThrowAsync(missingId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("No user found for a given id."));

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(new GetUserByIdQuery { Id = missingId }, CancellationToken.None),
            "GetUserByIdQueryHandler must propagate EntityNotFoundException when the user cannot be located");

        Assert.That(ex!.Message, Is.EqualTo("No user found for a given id."),
            "EntityNotFoundException should carry the id-based lookup message");
    }
}
