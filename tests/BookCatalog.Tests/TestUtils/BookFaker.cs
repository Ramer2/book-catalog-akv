using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Domain.Models;

namespace BookCatalog.Tests.TestUtils;

internal static class BookFaker
{
    public const string ValidIsbn = "1234567890";
    public const string ValidTitle = "The Pragmatic Programmer";
    public static readonly Guid ValidAuthorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const int ValidNumberOfPages = 352;

    public static readonly DateTime ValidPublishDate =
        new(1999, 10, 20, 0, 0, 0, DateTimeKind.Utc);

    public static Book Book(
        Guid? id = null,
        string? isbn = null,
        string? title = null,
        Guid? authorId = null,
        Author? author = null,
        int? numberOfPages = null,
        DateTime? publishDate = null)
    {
        var resolvedAuthorId = authorId ?? author?.Id ?? ValidAuthorId;
        var book = new Book(
            isbn ?? ValidIsbn,
            title ?? ValidTitle,
            resolvedAuthorId,
            numberOfPages ?? ValidNumberOfPages,
            publishDate ?? ValidPublishDate)
        {
            Id = id ?? Guid.NewGuid(),
            Author = author ?? AuthorFaker.Author(id: resolvedAuthorId)
        };

        return book;
    }

    public static CreateBookCommand CreateCommand(
        string? isbn = null,
        string? title = null,
        Guid? authorId = null,
        int? numberOfPages = null,
        DateTime? publishDate = null)
    {
        return new CreateBookCommand
        {
            Isbn = isbn ?? ValidIsbn,
            Title = title ?? ValidTitle,
            AuthorId = authorId ?? ValidAuthorId,
            NumberOfPages = numberOfPages ?? ValidNumberOfPages,
            PublishDate = publishDate ?? ValidPublishDate
        };
    }

    public static UpdateBookByIdCommand UpdateCommand(
        Guid? id = null,
        string? isbn = null,
        string? title = null,
        Guid? authorId = null,
        int? numberOfPages = null,
        DateTime? publishDate = null)
    {
        return new UpdateBookByIdCommand
        {
            Id = id ?? Guid.NewGuid(),
            Isbn = isbn ?? ValidIsbn,
            Title = title ?? ValidTitle,
            AuthorId = authorId ?? ValidAuthorId,
            NumberOfPages = numberOfPages ?? ValidNumberOfPages,
            PublishDate = publishDate ?? ValidPublishDate
        };
    }
}
