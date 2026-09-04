using AutoMapper;
using BookCatalog.Application.Handlers.User.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.User;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class UpdateUserByIdCommandHandlerTests
{
    private Mock<IUserService> _userService = null!;
    private Mock<IUserRepository> _userRepository = null!;
    private IMapper _mapper = null!;
    private UpdateUserByIdCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userService = new Mock<IUserService>(MockBehavior.Strict);
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _sut = new UpdateUserByIdCommandHandler(_mapper, _userService.Object, _userRepository.Object);
    }

    [Test]
    public async Task Handle_Should_MutateExistingUserAndReturnMappedResponse_WhenUserExists()
    {
        var existing = UserFaker.User(
            email: "old@example.com",
            phoneNumber: "+10000000000",
            firstName: "OldFirst",
            lastName: "OldLast",
            birthDate: new DateOnly(1980, 1, 1));

        _userService
            .Setup(s => s.GetOrThrowAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _userRepository
            .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = UserFaker.UpdateCommand(id: existing.Id);

        var response = await _sut.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(existing.Email, Is.EqualTo(command.Email),
                "Handler must overwrite User.Email from the command");
            Assert.That(existing.PhoneNumber, Is.EqualTo(command.PhoneNumber),
                "Handler must overwrite User.PhoneNumber from the command");
            Assert.That(existing.FirstName, Is.EqualTo(command.FirstName),
                "Handler must overwrite User.FirstName from the command");
            Assert.That(existing.LastName, Is.EqualTo(command.LastName),
                "Handler must overwrite User.LastName from the command");
            Assert.That(existing.BirthDate, Is.EqualTo(command.BirthDate),
                "Handler must overwrite User.BirthDate from the command");
            Assert.That(existing.Id, Is.EqualTo(command.Id),
                "Handler must not alter User.Id during update");
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(command.Id),
                "UserResponse.Id must reflect the updated user's Id");
            Assert.That(response.Email, Is.EqualTo(command.Email),
                "UserResponse.Email must reflect the newly assigned email");
            Assert.That(response.PhoneNumber, Is.EqualTo(command.PhoneNumber),
                "UserResponse.PhoneNumber must reflect the newly assigned phone number");
        });

        _userRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "UpdateUserByIdCommandHandler must flush changes via IUserRepository.SaveAsync exactly once");
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_UserDoesNotExist()
    {
        var command = UserFaker.UpdateCommand();
        _userService
            .Setup(s => s.GetOrThrowAsync(command.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("No user found for a given id."));

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(command, CancellationToken.None),
            "UpdateUserByIdCommandHandler must propagate EntityNotFoundException when the target user does not exist");

        Assert.That(ex!.Message, Is.EqualTo("No user found for a given id."),
            "EntityNotFoundException should carry the id-based lookup message from UserService.GetOrThrowAsync");

        _userRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "UpdateUserByIdCommandHandler must NOT call SaveAsync when the target user cannot be found");
    }
}
