using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.User;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private UserService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _sut = new UserService(_userRepository.Object);
    }

    [Test]
    public async Task GetOrThrowAsync_Should_ReturnUser_When_RepositoryReturnsMatch()
    {
        var expected = UserFaker.User();
        _userRepository
            .Setup(r => r.GetByIdAsync(expected.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var actual = await _sut.GetOrThrowAsync(expected.Id);

        Assert.That(actual, Is.SameAs(expected),
            "GetOrThrowAsync must return the exact instance yielded by the repository");
    }

    [Test]
    public void GetOrThrowAsync_Should_ThrowEntityNotFound_When_RepositoryReturnsNull()
    {
        var missingId = Guid.NewGuid();
        _userRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.GetOrThrowAsync(missingId),
            "GetOrThrowAsync must throw EntityNotFoundException when the repository has no matching user");

        Assert.That(ex!.Message, Is.EqualTo("No user found for a given id."),
            "EntityNotFoundException message should describe the id-based lookup failure");
    }

    // ---------- EnsureEmailUniqueAsync ----------

    [Test]
    [Category("Uniqueness")]
    public async Task EnsureEmailUniqueAsync_Should_ReturnTrue_When_NoUserHasThatEmail()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(UserFaker.ValidEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var isUnique = await _sut.EnsureEmailUniqueAsync(UserFaker.ValidEmail);

        Assert.That(isUnique, Is.True,
            "Email with no matching user in the repository should be considered available");
    }

    [Test]
    [Category("Uniqueness")]
    public async Task EnsureEmailUniqueAsync_Should_ReturnFalse_When_EmailTakenAndNoExclusion()
    {
        var existing = UserFaker.User(email: UserFaker.ValidEmail);
        _userRepository
            .Setup(r => r.GetByEmailAsync(UserFaker.ValidEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var isUnique = await _sut.EnsureEmailUniqueAsync(UserFaker.ValidEmail, excludeUserId: null);

        Assert.That(isUnique, Is.False,
            "When an existing user owns the email and no excludeUserId is provided, the email must be reported as taken");
    }

    [Test]
    [Category("Uniqueness")]
    public async Task EnsureEmailUniqueAsync_Should_ReturnTrue_When_EmailTakenByExcludedUser()
    {
        var existing = UserFaker.User(email: UserFaker.ValidEmail);
        _userRepository
            .Setup(r => r.GetByEmailAsync(UserFaker.ValidEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var isUnique = await _sut.EnsureEmailUniqueAsync(UserFaker.ValidEmail, excludeUserId: existing.Id);

        Assert.That(isUnique, Is.True,
            "When the email is owned by the user being updated (excludeUserId matches), it must be considered available");
    }

    [Test]
    [Category("Uniqueness")]
    public async Task EnsureEmailUniqueAsync_Should_ReturnFalse_When_EmailTakenByDifferentUserThanExcluded()
    {
        var existing = UserFaker.User(email: UserFaker.ValidEmail);
        _userRepository
            .Setup(r => r.GetByEmailAsync(UserFaker.ValidEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var otherUserId = Guid.NewGuid();

        var isUnique = await _sut.EnsureEmailUniqueAsync(UserFaker.ValidEmail, excludeUserId: otherUserId);

        Assert.That(isUnique, Is.False,
            "When the email is owned by a user whose Id differs from excludeUserId, the email must be reported as taken");
    }

    // ---------- EnsurePhoneNumberUniqueAsync ----------

    [Test]
    [Category("Uniqueness")]
    public async Task EnsurePhoneNumberUniqueAsync_Should_ReturnTrue_When_NoUserHasThatPhone()
    {
        _userRepository
            .Setup(r => r.GetByPhoneNumberAsync(UserFaker.ValidPhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var isUnique = await _sut.EnsurePhoneNumberUniqueAsync(UserFaker.ValidPhoneNumber);

        Assert.That(isUnique, Is.True,
            "Phone number with no matching user should be considered available");
    }

    [Test]
    [Category("Uniqueness")]
    public async Task EnsurePhoneNumberUniqueAsync_Should_ReturnFalse_When_PhoneTakenAndNoExclusion()
    {
        var existing = UserFaker.User(phoneNumber: UserFaker.ValidPhoneNumber);
        _userRepository
            .Setup(r => r.GetByPhoneNumberAsync(UserFaker.ValidPhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var isUnique = await _sut.EnsurePhoneNumberUniqueAsync(UserFaker.ValidPhoneNumber, excludeUserId: null);

        Assert.That(isUnique, Is.False,
            "When an existing user owns the phone number and no excludeUserId is provided, the phone must be reported as taken");
    }

    [Test]
    [Category("Uniqueness")]
    public async Task EnsurePhoneNumberUniqueAsync_Should_ReturnTrue_When_PhoneTakenByExcludedUser()
    {
        var existing = UserFaker.User(phoneNumber: UserFaker.ValidPhoneNumber);
        _userRepository
            .Setup(r => r.GetByPhoneNumberAsync(UserFaker.ValidPhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var isUnique = await _sut.EnsurePhoneNumberUniqueAsync(UserFaker.ValidPhoneNumber, excludeUserId: existing.Id);

        Assert.That(isUnique, Is.True,
            "When the phone is owned by the user being updated (excludeUserId matches), it must be considered available");
    }
}
