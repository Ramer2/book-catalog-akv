using System.Net;
using System.Net.Http.Json;
using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Application.Responses.Author;
using BookCatalog.Application.Responses.Book;

namespace BookCatalog.Tests.Integration;

public class AuthorAndBookIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task CreateAuthor_CreateBook_And_RetrieveBook_Should_Succeed_EndToEnd()
    {
        // 1. Create Author via POST /api/authors
        var createAuthorCommand = new CreateAuthorCommand
        {
            FirstName = "Robert",
            LastName = "Martin"
        };

        var authorHttpResponse = await Client.PostAsJsonAsync("/api/authors", createAuthorCommand);
        Assert.That(authorHttpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var createdAuthor = await authorHttpResponse.Content.ReadFromJsonAsync<AuthorResponse>();
        Assert.That(createdAuthor, Is.Not.Null);

        // 2. Create Book referencing created AuthorId via POST /api/books
        var createBookCommand = new CreateBookCommand
        {
            Isbn = "9780132350884",
            Title = "Clean Code",
            AuthorId = createdAuthor!.Id,
            NumberOfPages = 464,
            PublishDate = new DateTime(2008, 8, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var bookHttpResponse = await Client.PostAsJsonAsync("/api/books", createBookCommand);
        Assert.That(bookHttpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var createdBook = await bookHttpResponse.Content.ReadFromJsonAsync<BookResponse>();
        Assert.That(createdBook, Is.Not.Null);

        // 3. Retrieve created Book via GET /api/books/{id}
        var getBookHttpResponse = await Client.GetAsync($"/api/books/{createdBook!.Id}");
        Assert.That(getBookHttpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var fetchedBook = await getBookHttpResponse.Content.ReadFromJsonAsync<BookResponse>();
        Assert.That(fetchedBook, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(fetchedBook!.Id, Is.EqualTo(createdBook.Id));
            Assert.That(fetchedBook.Title, Is.EqualTo("Clean Code"));
            Assert.That(fetchedBook.Isbn, Is.EqualTo("9780132350884"));
            Assert.That(fetchedBook.Author, Is.Not.Null);
            Assert.That(fetchedBook.Author.Id, Is.EqualTo(createdAuthor.Id));
        });
    }
}
