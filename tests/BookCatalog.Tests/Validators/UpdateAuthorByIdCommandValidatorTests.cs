using BookCatalog.Application.Validators.Author.Command;
using BookCatalog.Tests.TestUtils;
using FluentValidation.TestHelper;

namespace BookCatalog.Tests.Validators;

[TestFixture]
[Category("Validation")]
public class UpdateAuthorByIdCommandValidatorTests
{
    private UpdateAuthorByIdCommandValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new UpdateAuthorByIdCommandValidator();
    }

    [Test]
    public async Task Should_PassValidation_When_CommandIsFullyValid()
    {
        var command = AuthorFaker.UpdateCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public async Task Id_Should_HaveRequiredError_When_Empty()
    {
        var command = AuthorFaker.UpdateCommand(id: Guid.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id is required.");
    }

    [Test]
    public async Task FirstName_Should_HaveRequiredError_When_Empty()
    {
        var command = AuthorFaker.UpdateCommand(firstName: string.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required.");
    }

    [Test]
    public async Task FirstName_Should_HaveLengthError_When_Exceeds100Characters()
    {
        var command = AuthorFaker.UpdateCommand(firstName: new string('a', 101));

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name must not exceed 100 characters.");
    }

    [Test]
    public async Task LastName_Should_HaveRequiredError_When_Empty()
    {
        var command = AuthorFaker.UpdateCommand(lastName: string.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required.");
    }

    [Test]
    public async Task LastName_Should_HaveLengthError_When_Exceeds100Characters()
    {
        var command = AuthorFaker.UpdateCommand(lastName: new string('a', 101));

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name must not exceed 100 characters.");
    }
}
