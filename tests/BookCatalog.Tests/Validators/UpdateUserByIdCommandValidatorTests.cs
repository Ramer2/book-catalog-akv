using BookCatalog.Application.Services.User;
using BookCatalog.Application.Validators.User.Command;
using BookCatalog.Tests.TestUtils;
using FluentValidation.TestHelper;

namespace BookCatalog.Tests.Validators;

[TestFixture]
[Category("Validation")]
public class UpdateUserByIdCommandValidatorTests
{
    private Mock<IUserService> _userService = null!;
    private UpdateUserByIdCommandValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userService = new Mock<IUserService>(MockBehavior.Strict);

        _userService
            .Setup(s => s.EnsureEmailUniqueAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userService
            .Setup(s => s.EnsurePhoneNumberUniqueAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sut = new UpdateUserByIdCommandValidator(_userService.Object);
    }

    [Test]
    public async Task Should_PassValidation_When_CommandIsFullyValid()
    {
        var command = UserFaker.UpdateCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public async Task Id_Should_HaveError_When_EmptyGuid()
    {
        var command = UserFaker.UpdateCommand(id: Guid.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id is required.");
    }

    [Test]
    public async Task Email_Should_InvokeUniquenessCheck_WithCommandId_ForUpdateCommand()
    {
        var command = UserFaker.UpdateCommand();

        await _sut.TestValidateAsync(command);

        _userService.Verify(
            s => s.EnsureEmailUniqueAsync(
                command.Email,
                command.Id,
                It.IsAny<CancellationToken>()),
            Times.Once,
            "UpdateUserByIdCommandValidator must call EnsureEmailUniqueAsync with excludeUserId=command.Id");
    }

    [Test]
    public async Task PhoneNumber_Should_InvokeUniquenessCheck_WithCommandId_ForUpdateCommand()
    {
        var command = UserFaker.UpdateCommand();

        await _sut.TestValidateAsync(command);

        _userService.Verify(
            s => s.EnsurePhoneNumberUniqueAsync(
                command.PhoneNumber,
                command.Id,
                It.IsAny<CancellationToken>()),
            Times.Once,
            "UpdateUserByIdCommandValidator must call EnsurePhoneNumberUniqueAsync with excludeUserId=command.Id");
    }

    [Test]
    public async Task Email_Should_HaveTakenError_When_UniquenessCheckReturnsFalse()
    {
        var command = UserFaker.UpdateCommand();

        _userService
            .Setup(s => s.EnsureEmailUniqueAsync(
                command.Email,
                command.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is already taken.");
    }

    [Test]
    public async Task PhoneNumber_Should_HaveTakenError_When_UniquenessCheckReturnsFalse()
    {
        var command = UserFaker.UpdateCommand();

        _userService
            .Setup(s => s.EnsurePhoneNumberUniqueAsync(
                command.PhoneNumber,
                command.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("Phone number is already taken.");
    }
}
