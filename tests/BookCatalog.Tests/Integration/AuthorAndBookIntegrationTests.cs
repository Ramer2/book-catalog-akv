using System.Net;
using System.Net.Http.Json;
using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Application.Responses.Author;
using BookCatalog.Application.Responses.Book;

namespace BookCatalog.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class AuthorAndBookIntegrationTests
{
    private PostgresFixture _postgresFixture = null!;
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgresFixture = new PostgresFixture();
        await _postgresFixture.InitializeAsync();

        _factory = new CustomWebApplicationFactory(_postgresFixture.ConnectionString);
        await _factory.EnsureDatabaseMigratedAsync();

        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _client?.Dispose();
        if (_factory != null)
            await _factory.DisposeAsync();

        if (_postgresFixture != null)
            await _postgresFixture.DisposeAsync();
    }

    [Test]
    public async Task CreateAuthor_CreateBook_And_RetrieveBook_Should_Succeed_EndToEnd()
    {
        // 1. Create Author via POST /api/authors
        var createAuthorCommand = new CreateAuthorCommand
        {
            FirstName = "Robert",
            LastName = "Martin"
        };

        var authorHttpResponse = await _client.PostAsJsonAsync("/api/authors", createAuthorCommand);
        Assert.That(authorHttpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "POST /api/authors must return 200 OK when creating a valid author");

        var createdAuthor = await authorHttpResponse.Content.ReadFromJsonAsync<AuthorResponse>();
        Assert.That(createdAuthor, Is.Not.Null, "Created author response must not be null");
        Assert.Multiple(() =>
        {
            Assert.That(createdAuthor!.Id, Is.Not.EqualTo(Guid.Empty), "Author Id must be populated");
            Assert.That(createdAuthor.FirstName, Is.EqualTo("Robert"));
            Assert.That(createdAuthor.LastName, Is.EqualTo("Martin"));
        });

        // 2. Create Book referencing the created AuthorId via POST /api/books
        var createBookCommand = new CreateBookCommand
        {
            Isbn = "9780132350884",
            Title = "Clean Code",
            AuthorId = createdAuthor.Id,
            NumberOfPages = 464,
            PublishDate = new DateTime(2008, 8, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var bookHttpResponse = await _client.PostAsJsonAsync("/api/books", createBookCommand);
        Assert.That(bookHttpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "POST /api/books must return 200 OK when creating a valid book with an existing AuthorId");

        var createdBook = await bookHttpResponse.Content.ReadFromJsonAsync<BookResponse>();
        Assert.That(createdBook, Is.Not.Null, "Created book response must not be null");
        Assert.Multiple(() =>
        {
            Assert.That(createdBook!.Id, Is.Not.EqualTo(Guid.Empty), "Book Id must be populated");
            Assert.That(createdBook.Title, Is.EqualTo("Clean Code"));
            Assert.That(createdBook.Isbn, Is.EqualTo("9780132350884"));
        });

        // 3. Retrieve the created Book via GET /api/books/{id}
        var getBookHttpResponse = await _client.GetAsync($"/api/books/{createdBook.Id}");
        Assert.That(getBookHttpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "GET /api/books/{id} must return 200 OK for an existing book");

        var fetchedBook = await getBookHttpResponse.Content.ReadFromJsonAsync<BookResponse>();
        Assert.That(fetchedBook, Is.Not.Null, "Fetched book response must not be null");

        Assert.Multiple(() =>
        {
            Assert.That(fetchedBook!.Id, Is.EqualTo(createdBook.Id), "Fetched Book.Id must match created Book.Id");
            Assert.That(fetchedBook.Title, Is.EqualTo("Clean Code"));
            Assert.That(fetchedBook.Isbn, Is.EqualTo("9780132350884"));
            Assert.That(fetchedBook.NumberOfPages, Is.EqualTo(464));
            Assert.That(fetchedBook.Author, Is.Not.Null, "Fetched Book must include populated Author object");
            Assert.That(fetchedBook.Author.Id, Is.EqualTo(createdAuthor.Id));
            Assert.That(fetchedBook.Author.FirstName, Is.EqualTo("Robert"));
            Assert.That(fetchedBook.Author.LastName, Is.EqualTo("Martin"));
        });
    }
}
