using BookCatalog.Application.Behaviors;
using FluentValidation;
using MediatR;
using ApplicationValidationException = BookCatalog.Application.Exceptions.ValidationException;

namespace BookCatalog.Tests.Behaviors;

[TestFixture]
[Category("Validation")]
public class ValidationBehaviorTests
{
    private class SampleRequest : IRequest<int>
    {
        public string Name { get; set; } = null!;
    }

    private sealed class AlwaysValidValidator : AbstractValidator<SampleRequest>
    {
    }

    private sealed class RequiresNameValidator : AbstractValidator<SampleRequest>
    {
        public RequiresNameValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        }
    }

    private static RequestHandlerDelegate<int> MakeNext(int result, Action? onCalled = null)
    {
        return (CancellationToken _) =>
        {
            onCalled?.Invoke();
            return Task.FromResult(result);
        };
    }

    [Test]
    public async Task Handle_Should_InvokeNext_When_NoValidatorsRegistered()
    {
        var behavior = new ValidationBehavior<SampleRequest, int>(
            Enumerable.Empty<IValidator<SampleRequest>>());

        var nextCalled = false;
        var next = MakeNext(42, () => nextCalled = true);

        var result = await behavior.Handle(new SampleRequest { Name = "anything" }, next, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(nextCalled, Is.True,
                "ValidationBehavior must invoke next() when no validators are registered");
            Assert.That(result, Is.EqualTo(42),
                "ValidationBehavior must return whatever next() produced");
        });
    }

    [Test]
    public async Task Handle_Should_InvokeNext_When_AllValidatorsPass()
    {
        var behavior = new ValidationBehavior<SampleRequest, int>(
            new IValidator<SampleRequest>[] { new AlwaysValidValidator() });

        var nextCalled = false;
        var next = MakeNext(7, () => nextCalled = true);

        var result = await behavior.Handle(new SampleRequest { Name = "ok" }, next, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(nextCalled, Is.True,
                "ValidationBehavior must invoke next() when every validator reports success");
            Assert.That(result, Is.EqualTo(7),
                "ValidationBehavior must return next()'s result when validation succeeds");
        });
    }

    [Test]
    public void Handle_Should_ThrowValidationExceptionAndSkipNext_When_ValidatorFails()
    {
        var behavior = new ValidationBehavior<SampleRequest, int>(
            new IValidator<SampleRequest>[] { new RequiresNameValidator() });

        var nextCalled = false;
        var next = MakeNext(99, () => nextCalled = true);

        var ex = Assert.ThrowsAsync<ApplicationValidationException>(
            () => behavior.Handle(new SampleRequest { Name = string.Empty }, next, CancellationToken.None),
            "ValidationBehavior must throw the application ValidationException when a validator reports failures");

        Assert.Multiple(() =>
        {
            Assert.That(nextCalled, Is.False,
                "ValidationBehavior must NOT invoke next() when validation fails");
            Assert.That(ex!.Errors, Does.ContainKey(nameof(SampleRequest.Name)),
                "ValidationException.Errors must contain an entry for the failing property");
            Assert.That(ex.Errors[nameof(SampleRequest.Name)], Does.Contain("Name is required."),
                "ValidationException.Errors must expose the validator's error message verbatim");
        });
    }
}
