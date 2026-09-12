using System.Net;
using System.Net.Http.Json;
using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Application.Responses.Author;
using BookCatalog.Application.Responses.Book;

namespace BookCatalog.Tests.Integration;

public class BooksIntegrationTests : IntegrationTestBase
{
    private async Task<AuthorResponse> SeedAuthorAsync(string firstName = "Martin", string lastName = "Fowler")
    {
        var resp = await Client.PostAsJsonAsync("/api/authors", new CreateAuthorCommand { FirstName = firstName, LastName = lastName });
        return (await resp.Content.ReadFromJsonAsync<AuthorResponse>())!;
    }

    [Test]
    public async Task CreateBook_Should_Return400_When_AuthorIdDoesNotExist()
    {
        var command = new CreateBookCommand
        {
            Isbn = "1234567890",
            Title = "Refactoring",
            AuthorId = Guid.NewGuid(),
            NumberOfPages = 400
        };

        var response = await Client.PostAsJsonAsync("/api/books", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateBook_Should_Return400_When_IsbnIsDuplicate()
    {
        var author = await SeedAuthorAsync();
        var command1 = new CreateBookCommand { Isbn = "1234567890123", Title = "Book 1", AuthorId = author.Id, NumberOfPages = 100 };
        var command2 = new CreateBookCommand { Isbn = "1234567890123", Title = "Book 2", AuthorId = author.Id, NumberOfPages = 200 };

        var resp1 = await Client.PostAsJsonAsync("/api/books", command1);
        Assert.That(resp1.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var resp2 = await Client.PostAsJsonAsync("/api/books", command2);
        Assert.That(resp2.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetBookByIsbn_Should_Return200_When_BookExists()
    {
        var author = await SeedAuthorAsync();
        var command = new CreateBookCommand { Isbn = "9780201633610", Title = "Design Patterns", AuthorId = author.Id, NumberOfPages = 395 };
        await Client.PostAsJsonAsync("/api/books", command);

        var response = await Client.GetAsync("/api/books/isbn/9780201633610");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.That(book, Is.Not.Null);
        Assert.That(book!.Title, Is.EqualTo("Design Patterns"));
    }

    [Test]
    public async Task GetBookByIsbn_Should_Return404_When_IsbnNotFound()
    {
        var response = await Client.GetAsync("/api/books/isbn/0000000000");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetAllBooks_Should_ReturnPagedList_WithFilters()
    {
        var author = await SeedAuthorAsync();
        await Client.PostAsJsonAsync("/api/books", new CreateBookCommand { Isbn = "1111111111", Title = "Domain-Driven Design", AuthorId = author.Id, NumberOfPages = 500 });
        await Client.PostAsJsonAsync("/api/books", new CreateBookCommand { Isbn = "2222222222", Title = "Clean Architecture", AuthorId = author.Id, NumberOfPages = 400 });

        var response = await Client.GetAsync("/api/books?page=1&pageSize=10&title=Domain");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var paged = await response.Content.ReadFromJsonAsync<GetAllBooksResponse>();
        Assert.That(paged, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(paged!.TotalCount, Is.EqualTo(1));
            Assert.That(paged.Items.First().Title, Is.EqualTo("Domain-Driven Design"));
        });
    }

    [Test]
    public async Task UpdateBookById_Should_UpdateBookDetails_When_Valid()
    {
        var author = await SeedAuthorAsync();
        var createResp = await Client.PostAsJsonAsync("/api/books", new CreateBookCommand { Isbn = "3333333333", Title = "Old Title", AuthorId = author.Id, NumberOfPages = 100 });
        var created = await createResp.Content.ReadFromJsonAsync<BookResponse>();

        var updateCmd = new UpdateBookByIdCommand
        {
            Id = created!.Id,
            Isbn = "3333333333",
            Title = "New Title",
            AuthorId = author.Id,
            NumberOfPages = 150
        };

        var response = await Client.PutAsJsonAsync($"/api/books/{created.Id}", updateCmd);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(updated!.Title, Is.EqualTo("New Title"));
            Assert.That(updated.NumberOfPages, Is.EqualTo(150));
        });
    }

    [Test]
    public async Task DeleteBookById_Should_RemoveBook_AndReturn204()
    {
        var author = await SeedAuthorAsync();
        var createResp = await Client.PostAsJsonAsync("/api/books", new CreateBookCommand { Isbn = "4444444444", Title = "Temp Book", AuthorId = author.Id, NumberOfPages = 100 });
        var created = await createResp.Content.ReadFromJsonAsync<BookResponse>();

        var deleteResp = await Client.DeleteAsync($"/api/books/{created!.Id}");

        Assert.That(deleteResp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getResp = await Client.GetAsync($"/api/books/{created.Id}");
        Assert.That(getResp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteBookById_Should_Return404_When_NotFound()
    {
        var missingId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"/api/books/{missingId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
