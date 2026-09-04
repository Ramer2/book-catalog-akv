using BookCatalog.Application.Services.User;
using BookCatalog.Application.Validators.User.Command;
using BookCatalog.Tests.TestUtils;
using FluentValidation.TestHelper;

namespace BookCatalog.Tests.Validators;

[TestFixture]
[Category("Validation")]
public class CreateUserCommandValidatorTests
{
    private Mock<IUserService> _userService = null!;
    private CreateUserCommandValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userService = new Mock<IUserService>(MockBehavior.Strict);

        // Defaults: email and phone are available. Individual tests can override.
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

        _sut = new CreateUserCommandValidator(_userService.Object);
    }

    [Test]
    public async Task Should_PassValidation_When_CommandIsFullyValid()
    {
        var command = UserFaker.CreateCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ---------- Email ----------

    [Test]
    public async Task Email_Should_HaveRequiredError_When_Empty()
    {
        var command = UserFaker.CreateCommand(email: string.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Test]
    public async Task Email_Should_NotInvokeUniquenessCheck_When_Empty()
    {
        var command = UserFaker.CreateCommand(email: string.Empty);

        await _sut.TestValidateAsync(command);

        _userService.Verify(
            s => s.EnsureEmailUniqueAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never,
            "CascadeMode.Stop must prevent the async uniqueness check when NotEmpty fails");
    }

    [Test]
    public async Task Email_Should_HaveFormatError_When_MalformedAddress()
    {
        var command = UserFaker.CreateCommand(email: "not-an-email");

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is not a valid email address.");
    }

    [Test]
    public async Task Email_Should_HaveTakenError_When_UniquenessCheckReturnsFalse()
    {
        _userService
            .Setup(s => s.EnsureEmailUniqueAsync(
                UserFaker.ValidEmail,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = UserFaker.CreateCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is already taken.");
    }

    [Test]
    public async Task Email_Should_InvokeUniquenessCheck_WithNullExcludeUserId_ForCreateCommand()
    {
        var command = UserFaker.CreateCommand();

        await _sut.TestValidateAsync(command);

        _userService.Verify(
            s => s.EnsureEmailUniqueAsync(
                command.Email,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once,
            "CreateUserCommandValidator must call EnsureEmailUniqueAsync with excludeUserId=null");
    }

    // ---------- Phone number ----------

    [Test]
    public async Task PhoneNumber_Should_HaveRequiredError_When_Empty()
    {
        var command = UserFaker.CreateCommand(phoneNumber: string.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("Phone number is required.");
    }

    [TestCase("abc")]
    [TestCase("12")]
    [TestCase("phone!number")]
    public async Task PhoneNumber_Should_HaveFormatError_When_DoesNotMatchRegex(string phone)
    {
        var command = UserFaker.CreateCommand(phoneNumber: phone);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("Phone number is not in a valid format.");
    }

    [Test]
    public async Task PhoneNumber_Should_HaveTakenError_When_UniquenessCheckReturnsFalse()
    {
        _userService
            .Setup(s => s.EnsurePhoneNumberUniqueAsync(
                UserFaker.ValidPhoneNumber,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = UserFaker.CreateCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("Phone number is already taken.");
    }

    [Test]
    public async Task PhoneNumber_Should_NotInvokeUniquenessCheck_When_FormatInvalid()
    {
        var command = UserFaker.CreateCommand(phoneNumber: "abc");

        await _sut.TestValidateAsync(command);

        _userService.Verify(
            s => s.EnsurePhoneNumberUniqueAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never,
            "CascadeMode.Stop must prevent the async uniqueness check when phone format fails");
    }

    // ---------- Names ----------

    [Test]
    public async Task FirstName_Should_HaveRequiredError_When_Empty()
    {
        var command = UserFaker.CreateCommand(firstName: string.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required.");
    }

    [Test]
    public async Task FirstName_Should_HaveLengthError_When_Exceeds100Characters()
    {
        var command = UserFaker.CreateCommand(firstName: new string('a', 101));

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name must not exceed 100 characters.");
    }

    [Test]
    public async Task LastName_Should_HaveRequiredError_When_Empty()
    {
        var command = UserFaker.CreateCommand(lastName: string.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required.");
    }

    [Test]
    public async Task LastName_Should_HaveLengthError_When_Exceeds100Characters()
    {
        var command = UserFaker.CreateCommand(lastName: new string('a', 101));

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name must not exceed 100 characters.");
    }

    // ---------- BirthDate ----------

    [Test]
    public async Task BirthDate_Should_HaveError_When_InTheFuture()
    {
        var future = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var command = UserFaker.CreateCommand(birthDate: future);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
            .WithErrorMessage("Birth date cannot be in the future.");
    }

    [Test]
    public async Task BirthDate_Should_HaveError_When_OnOrBefore1900()
    {
        var command = UserFaker.CreateCommand(birthDate: new DateOnly(1900, 1, 1));

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
            .WithErrorMessage("Birth date must be after 1900-01-01.");
    }

    [Test]
    public async Task BirthDate_Should_NotHaveError_When_ValidPastDate()
    {
        var command = UserFaker.CreateCommand(birthDate: new DateOnly(1985, 6, 15));

        var result = await _sut.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }
}
