using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Domain.Models;

namespace BookCatalog.Tests.TestUtils;

internal static class AuthorFaker
{
    public const string ValidFirstName = "Martin";
    public const string ValidLastName = "Fowler";

    public static Author Author(
        Guid? id = null,
        string? firstName = null,
        string? lastName = null)
    {
        return new Author(
            firstName ?? ValidFirstName,
            lastName ?? ValidLastName)
        {
            Id = id ?? Guid.NewGuid()
        };
    }

    public static CreateAuthorCommand CreateCommand(
        string? firstName = null,
        string? lastName = null)
    {
        return new CreateAuthorCommand
        {
            FirstName = firstName ?? ValidFirstName,
            LastName = lastName ?? ValidLastName
        };
    }

    public static UpdateAuthorByIdCommand UpdateCommand(
        Guid? id = null,
        string? firstName = null,
        string? lastName = null)
    {
        return new UpdateAuthorByIdCommand
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = firstName ?? ValidFirstName,
            LastName = lastName ?? ValidLastName
        };
    }
}
