using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Domain.Models;

namespace BookCatalog.Tests.TestUtils;

/// <summary>
/// Deterministic sample data. Keep values distinct so a failing assertion
/// clearly shows which field diverged.
/// </summary>
internal static class BookFaker
{
    public const string ValidIsbn = "1234567890";
    public const string ValidTitle = "The Pragmatic Programmer";
    public const string ValidAuthor = "Andy Hunt";
    public const int ValidNumberOfPages = 352;

    public static readonly DateTime ValidPublishDate =
        new(1999, 10, 20, 0, 0, 0, DateTimeKind.Utc);

    public static Book Book(
        Guid? id = null,
        string? isbn = null,
        string? title = null,
        string? author = null,
        int? numberOfPages = null,
        DateTime? publishDate = null)
    {
        return new Book(
            isbn ?? ValidIsbn,
            title ?? ValidTitle,
            author ?? ValidAuthor,
            numberOfPages ?? ValidNumberOfPages,
            publishDate ?? ValidPublishDate)
        {
            Id = id ?? Guid.NewGuid()
        };
    }

    public static CreateBookCommand CreateCommand(
        string? isbn = null,
        string? title = null,
        string? author = null,
        int? numberOfPages = null,
        DateTime? publishDate = null)
    {
        return new CreateBookCommand
        {
            Isbn = isbn ?? ValidIsbn,
            Title = title ?? ValidTitle,
            Author = author ?? ValidAuthor,
            NumberOfPages = numberOfPages ?? ValidNumberOfPages,
            PublishDate = publishDate ?? ValidPublishDate
        };
    }

    public static UpdateBookByIdCommand UpdateCommand(
        Guid? id = null,
        string? isbn = null,
        string? title = null,
        string? author = null,
        int? numberOfPages = null,
        DateTime? publishDate = null)
    {
        return new UpdateBookByIdCommand
        {
            Id = id ?? Guid.NewGuid(),
            Isbn = isbn ?? ValidIsbn,
            Title = title ?? ValidTitle,
            Author = author ?? ValidAuthor,
            NumberOfPages = numberOfPages ?? ValidNumberOfPages,
            PublishDate = publishDate ?? ValidPublishDate
        };
    }
}
