using AutoMapper;
using BookCatalog.Application.Handlers.User.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class CreateUserCommandHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private IMapper _mapper = null!;
    private CreateUserCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _sut = new CreateUserCommandHandler(_mapper, _userRepository.Object);
    }

    [Test]
    public async Task Handle_Should_PersistUserAndReturnMappedResponse_WhenCommandIsValid()
    {
        User? captured = null;
        _userRepository
            .Setup(r => r.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => captured = u)
            .Returns(Task.CompletedTask);

        var command = UserFaker.CreateCommand();

        var response = await _sut.Handle(command, CancellationToken.None);

        Assert.That(captured, Is.Not.Null,
            "CreateUserCommandHandler must invoke IUserRepository.InsertAsync with the new user");

        Assert.Multiple(() =>
        {
            Assert.That(captured!.Email, Is.EqualTo(command.Email),
                "Inserted User.Email must mirror CreateUserCommand.Email");
            Assert.That(captured.PhoneNumber, Is.EqualTo(command.PhoneNumber),
                "Inserted User.PhoneNumber must mirror CreateUserCommand.PhoneNumber");
            Assert.That(captured.FirstName, Is.EqualTo(command.FirstName),
                "Inserted User.FirstName must mirror CreateUserCommand.FirstName");
            Assert.That(captured.LastName, Is.EqualTo(command.LastName),
                "Inserted User.LastName must mirror CreateUserCommand.LastName");
            Assert.That(captured.BirthDate, Is.EqualTo(command.BirthDate),
                "Inserted User.BirthDate must mirror CreateUserCommand.BirthDate");
            Assert.That(captured.CreatedAt, Is.Not.EqualTo(default(DateTime)),
                "CreatedAt must be stamped by the domain constructor at creation time");
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.Email, Is.EqualTo(command.Email),
                "UserResponse.Email must reflect the persisted user's email");
            Assert.That(response.PhoneNumber, Is.EqualTo(command.PhoneNumber),
                "UserResponse.PhoneNumber must reflect the persisted user's phone number");
            Assert.That(response.FirstName, Is.EqualTo(command.FirstName),
                "UserResponse.FirstName must reflect the persisted user's first name");
            Assert.That(response.LastName, Is.EqualTo(command.LastName),
                "UserResponse.LastName must reflect the persisted user's last name");
            Assert.That(response.BirthDate, Is.EqualTo(command.BirthDate),
                "UserResponse.BirthDate must reflect the persisted user's birth date");
        });

        _userRepository.Verify(
            r => r.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "CreateUserCommandHandler must persist the new user exactly once");
    }
}
