using BookCatalog.Application.Handlers.User.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.User.Command;
using BookCatalog.Application.Services.User;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class DeleteUserByIdCommandHandlerTests
{
    private Mock<IUserService> _userService = null!;
    private Mock<IUserRepository> _userRepository = null!;
    private DeleteUserByIdCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userService = new Mock<IUserService>(MockBehavior.Strict);
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _sut = new DeleteUserByIdCommandHandler(_userService.Object, _userRepository.Object);
    }

    [Test]
    public async Task Handle_Should_DeleteUser_WhenUserExists()
    {
        var existing = UserFaker.User();
        _userService
            .Setup(s => s.GetOrThrowAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _userRepository
            .Setup(r => r.DeleteEntityAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(new DeleteUserByIdCommand { Id = existing.Id }, CancellationToken.None);

        _userRepository.Verify(
            r => r.DeleteEntityAsync(existing, It.IsAny<CancellationToken>()),
            Times.Once,
            "DeleteUserByIdCommandHandler must delete the exact instance returned by GetOrThrowAsync");
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_UserDoesNotExist()
    {
        var command = new DeleteUserByIdCommand { Id = Guid.NewGuid() };
        _userService
            .Setup(s => s.GetOrThrowAsync(command.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("No user found for a given id."));

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(command, CancellationToken.None),
            "DeleteUserByIdCommandHandler must propagate EntityNotFoundException when the target user does not exist");

        Assert.That(ex!.Message, Is.EqualTo("No user found for a given id."),
            "EntityNotFoundException should carry the id-based lookup message");

        _userRepository.Verify(
            r => r.DeleteEntityAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "DeleteUserByIdCommandHandler must NOT call DeleteEntityAsync when the target user cannot be found");
    }
}
