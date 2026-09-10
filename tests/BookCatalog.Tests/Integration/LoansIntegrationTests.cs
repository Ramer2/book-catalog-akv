using System.Net;
using System.Net.Http.Json;
using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Application.Requests.Loan.Command;
using BookCatalog.Application.Requests.User.Command;
using BookCatalog.Application.Responses.Author;
using BookCatalog.Application.Responses.Book;
using BookCatalog.Application.Responses.Loan;
using BookCatalog.Application.Responses.User;

namespace BookCatalog.Tests.Integration;

public class LoansIntegrationTests : IntegrationTestBase
{
    private async Task<(UserResponse User, BookResponse Book)> SeedUserAndBookAsync()
    {
        var authorResp = await Client.PostAsJsonAsync("/api/authors", new CreateAuthorCommand { FirstName = "Eric", LastName = "Evans" });
        var author = (await authorResp.Content.ReadFromJsonAsync<AuthorResponse>())!;

        var bookResp = await Client.PostAsJsonAsync("/api/books", new CreateBookCommand { Isbn = Guid.NewGuid().ToString("N")[..13], Title = "DDD", AuthorId = author.Id, NumberOfPages = 500 });
        var book = (await bookResp.Content.ReadFromJsonAsync<BookResponse>())!;

        var userResp = await Client.PostAsJsonAsync("/api/users", new CreateUserCommand { Email = $"{Guid.NewGuid():N}@ex.com", PhoneNumber = "+1555" + Random.Shared.Next(1000000, 9999999), FirstName = "Test", LastName = "User", BirthDate = new DateOnly(1990, 1, 1) });
        var user = (await userResp.Content.ReadFromJsonAsync<UserResponse>())!;

        return (user, book);
    }

    [Test]
    public async Task BorrowBook_Should_Return200_When_BookIsAvailable()
    {
        var (user, book) = await SeedUserAndBookAsync();

        var borrowCmd = new BorrowBookCommand { BookId = book.Id, UserId = user.Id };
        var response = await Client.PostAsJsonAsync("/api/loans", borrowCmd);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var loan = await response.Content.ReadFromJsonAsync<LoanResponse>();
        Assert.That(loan, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(loan!.BookId, Is.EqualTo(book.Id));
            Assert.That(loan.UserId, Is.EqualTo(user.Id));
            Assert.That(loan.ReturnedAt, Is.Null);
        });
    }

    [Test]
    public async Task BorrowBook_Should_Return400BadRequest_When_BookIsAlreadyBorrowed()
    {
        var (user1, book) = await SeedUserAndBookAsync();
        var user2Resp = await Client.PostAsJsonAsync("/api/users", new CreateUserCommand { Email = "user2@ex.com", PhoneNumber = "+15559990000", FirstName = "User2", LastName = "Two", BirthDate = new DateOnly(1992, 2, 2) });
        var user2 = (await user2Resp.Content.ReadFromJsonAsync<UserResponse>())!;

        // First borrow succeeds
        var borrow1 = await Client.PostAsJsonAsync("/api/loans", new BorrowBookCommand { BookId = book.Id, UserId = user1.Id });
        Assert.That(borrow1.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Second borrow attempt on the same book while loan is active fails validation with 400 Bad Request
        var borrow2 = await Client.PostAsJsonAsync("/api/loans", new BorrowBookCommand { BookId = book.Id, UserId = user2.Id });
        Assert.That(borrow2.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ReturnLoan_Should_Return200_AndSetReturnedAt()
    {
        var (user, book) = await SeedUserAndBookAsync();
        var borrowResp = await Client.PostAsJsonAsync("/api/loans", new BorrowBookCommand { BookId = book.Id, UserId = user.Id });
        var loan = (await borrowResp.Content.ReadFromJsonAsync<LoanResponse>())!;

        var returnResp = await Client.PatchAsync($"/api/loans/{loan.Id}/return", null);

        Assert.That(returnResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var returnedLoan = await returnResp.Content.ReadFromJsonAsync<LoanResponse>();
        Assert.That(returnedLoan, Is.Not.Null);
        Assert.That(returnedLoan!.ReturnedAt, Is.Not.Null);
    }

    [Test]
    public async Task ReturnLoan_Should_Return404_When_LoanDoesNotExist()
    {
        var missingLoanId = Guid.NewGuid();

        var response = await Client.PatchAsync($"/api/loans/{missingLoanId}/return", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetLoanById_Should_Return200_When_LoanExists()
    {
        var (user, book) = await SeedUserAndBookAsync();
        var borrowResp = await Client.PostAsJsonAsync("/api/loans", new BorrowBookCommand { BookId = book.Id, UserId = user.Id });
        var created = (await borrowResp.Content.ReadFromJsonAsync<LoanResponse>())!;

        var response = await Client.GetAsync($"/api/loans/{created.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await response.Content.ReadFromJsonAsync<LoanResponse>();
        Assert.That(fetched!.Id, Is.EqualTo(created.Id));
    }

    [Test]
    public async Task GetLoanById_Should_Return404_When_NotFound()
    {
        var response = await Client.GetAsync($"/api/loans/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetUserLoans_Should_ReturnUserBorrowingHistory()
    {
        var (user, book) = await SeedUserAndBookAsync();
        await Client.PostAsJsonAsync("/api/loans", new BorrowBookCommand { BookId = book.Id, UserId = user.Id });

        var response = await Client.GetAsync($"/api/users/{user.Id}/loans?page=1&pageSize=10");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var paged = await response.Content.ReadFromJsonAsync<GetAllLoansResponse>();
        Assert.That(paged, Is.Not.Null);
        Assert.That(paged!.TotalCount, Is.EqualTo(1));
    }
}
