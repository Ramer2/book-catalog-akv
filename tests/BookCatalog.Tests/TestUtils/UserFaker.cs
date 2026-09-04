using BookCatalog.Application.Requests.User.Command;
using BookCatalog.Domain.Models;

namespace BookCatalog.Tests.TestUtils;

/// <summary>
/// Deterministic sample data for User-related tests. Keep values distinct
/// so a failing assertion clearly shows which field diverged.
/// </summary>
internal static class UserFaker
{
    public const string ValidEmail = "john.doe@example.com";
    public const string ValidPhoneNumber = "+15551234567";
    public const string ValidFirstName = "John";
    public const string ValidLastName = "Doe";

    public static readonly DateOnly ValidBirthDate = new(1990, 5, 20);

    public static User User(
        Guid? id = null,
        string? email = null,
        string? phoneNumber = null,
        string? firstName = null,
        string? lastName = null,
        DateOnly? birthDate = null)
    {
        return new User(
            email ?? ValidEmail,
            phoneNumber ?? ValidPhoneNumber,
            firstName ?? ValidFirstName,
            lastName ?? ValidLastName,
            birthDate ?? ValidBirthDate)
        {
            Id = id ?? Guid.NewGuid()
        };
    }

    public static CreateUserCommand CreateCommand(
        string? email = null,
        string? phoneNumber = null,
        string? firstName = null,
        string? lastName = null,
        DateOnly? birthDate = null)
    {
        return new CreateUserCommand
        {
            Email = email ?? ValidEmail,
            PhoneNumber = phoneNumber ?? ValidPhoneNumber,
            FirstName = firstName ?? ValidFirstName,
            LastName = lastName ?? ValidLastName,
            BirthDate = birthDate ?? ValidBirthDate
        };
    }

    public static UpdateUserByIdCommand UpdateCommand(
        Guid? id = null,
        string? email = null,
        string? phoneNumber = null,
        string? firstName = null,
        string? lastName = null,
        DateOnly? birthDate = null)
    {
        return new UpdateUserByIdCommand
        {
            Id = id ?? Guid.NewGuid(),
            Email = email ?? ValidEmail,
            PhoneNumber = phoneNumber ?? ValidPhoneNumber,
            FirstName = firstName ?? ValidFirstName,
            LastName = lastName ?? ValidLastName,
            BirthDate = birthDate ?? ValidBirthDate
        };
    }
}
