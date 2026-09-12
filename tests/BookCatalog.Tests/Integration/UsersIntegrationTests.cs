using System.Net;
using System.Net.Http.Json;
using BookCatalog.Application.Requests.User.Command;
using BookCatalog.Application.Responses.User;

namespace BookCatalog.Tests.Integration;

public class UsersIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task CreateUser_Should_Return200_When_UserIsValid()
    {
        var command = new CreateUserCommand
        {
            Email = "john.doe@example.com",
            PhoneNumber = "+15551234567",
            FirstName = "John",
            LastName = "Doe",
            BirthDate = new DateOnly(1990, 1, 1)
        };

        var response = await Client.PostAsJsonAsync("/api/users", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.That(user, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(user!.Email, Is.EqualTo("john.doe@example.com"));
            Assert.That(user.FirstName, Is.EqualTo("John"));
        });
    }

    [Test]
    public async Task CreateUser_Should_Return400_When_EmailIsDuplicate()
    {
        var command1 = new CreateUserCommand { Email = "dup@example.com", PhoneNumber = "+11111111111", FirstName = "User1", LastName = "One", BirthDate = new DateOnly(1990, 1, 1) };
        var command2 = new CreateUserCommand { Email = "dup@example.com", PhoneNumber = "+22222222222", FirstName = "User2", LastName = "Two", BirthDate = new DateOnly(1990, 1, 1) };

        var resp1 = await Client.PostAsJsonAsync("/api/users", command1);
        Assert.That(resp1.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var resp2 = await Client.PostAsJsonAsync("/api/users", command2);
        Assert.That(resp2.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateUser_Should_Return400_When_PhoneIsDuplicate()
    {
        var command1 = new CreateUserCommand { Email = "u1@example.com", PhoneNumber = "+19998887777", FirstName = "User1", LastName = "One", BirthDate = new DateOnly(1990, 1, 1) };
        var command2 = new CreateUserCommand { Email = "u2@example.com", PhoneNumber = "+19998887777", FirstName = "User2", LastName = "Two", BirthDate = new DateOnly(1990, 1, 1) };

        var resp1 = await Client.PostAsJsonAsync("/api/users", command1);
        Assert.That(resp1.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var resp2 = await Client.PostAsJsonAsync("/api/users", command2);
        Assert.That(resp2.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetUserById_Should_Return200_When_UserExists()
    {
        var createCmd = new CreateUserCommand { Email = "alice@example.com", PhoneNumber = "+12345678901", FirstName = "Alice", LastName = "Smith", BirthDate = new DateOnly(1995, 5, 5) };
        var createResp = await Client.PostAsJsonAsync("/api/users", createCmd);
        var created = await createResp.Content.ReadFromJsonAsync<UserResponse>();

        var response = await Client.GetAsync($"/api/users/{created!.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.That(user!.Id, Is.EqualTo(created.Id));
    }

    [Test]
    public async Task GetUserById_Should_Return404_When_NotFound()
    {
        var response = await Client.GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetAllUsers_Should_ReturnPagedList_WithFilters()
    {
        await Client.PostAsJsonAsync("/api/users", new CreateUserCommand { Email = "bob@example.com", PhoneNumber = "+12345678902", FirstName = "Bob", LastName = "Jones", BirthDate = new DateOnly(1985, 3, 3) });

        var response = await Client.GetAsync("/api/users?page=1&pageSize=10&email=bob");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var paged = await response.Content.ReadFromJsonAsync<GetAllUsersResponse>();
        Assert.That(paged, Is.Not.Null);
        Assert.That(paged!.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateUserById_Should_MutateUser_When_Valid()
    {
        var createResp = await Client.PostAsJsonAsync("/api/users", new CreateUserCommand { Email = "charlie@example.com", PhoneNumber = "+12345678903", FirstName = "Charlie", LastName = "Brown", BirthDate = new DateOnly(2000, 1, 1) });
        var created = await createResp.Content.ReadFromJsonAsync<UserResponse>();

        var updateCmd = new UpdateUserByIdCommand
        {
            Id = created!.Id,
            Email = "charlie.updated@example.com",
            PhoneNumber = "+12345678903",
            FirstName = "Charles",
            LastName = "Brown",
            BirthDate = new DateOnly(2000, 1, 1)
        };

        var response = await Client.PutAsJsonAsync($"/api/users/{created.Id}", updateCmd);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(updated!.Email, Is.EqualTo("charlie.updated@example.com"));
            Assert.That(updated.FirstName, Is.EqualTo("Charles"));
        });
    }

    [Test]
    public async Task DeleteUserById_Should_RemoveUser_AndReturn204()
    {
        var createResp = await Client.PostAsJsonAsync("/api/users", new CreateUserCommand { Email = "del@example.com", PhoneNumber = "+12345678904", FirstName = "Del", LastName = "User", BirthDate = new DateOnly(1999, 9, 9) });
        var created = await createResp.Content.ReadFromJsonAsync<UserResponse>();

        var deleteResp = await Client.DeleteAsync($"/api/users/{created!.Id}");

        Assert.That(deleteResp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getResp = await Client.GetAsync($"/api/users/{created.Id}");
        Assert.That(getResp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
