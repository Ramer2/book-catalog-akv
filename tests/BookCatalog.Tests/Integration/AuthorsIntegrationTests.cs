using System.Net;
using System.Net.Http.Json;
using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Application.Responses.Author;

namespace BookCatalog.Tests.Integration;

public class AuthorsIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task CreateAuthor_Should_Return200_When_CommandIsValid()
    {
        var command = new CreateAuthorCommand
        {
            FirstName = "Martin",
            LastName = "Fowler"
        };

        var response = await Client.PostAsJsonAsync("/api/authors", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var author = await response.Content.ReadFromJsonAsync<AuthorResponse>();
        Assert.That(author, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(author!.FirstName, Is.EqualTo("Martin"));
            Assert.That(author.LastName, Is.EqualTo("Fowler"));
            Assert.That(author.Id, Is.Not.EqualTo(Guid.Empty));
        });
    }

    [Test]
    public async Task CreateAuthor_Should_Return400_When_FirstNameIsEmpty()
    {
        var command = new CreateAuthorCommand
        {
            FirstName = "",
            LastName = "Fowler"
        };

        var response = await Client.PostAsJsonAsync("/api/authors", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetAuthorById_Should_Return200_When_AuthorExists()
    {
        var createCmd = new CreateAuthorCommand { FirstName = "Kent", LastName = "Beck" };
        var createResp = await Client.PostAsJsonAsync("/api/authors", createCmd);
        var created = await createResp.Content.ReadFromJsonAsync<AuthorResponse>();

        var response = await Client.GetAsync($"/api/authors/{created!.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await response.Content.ReadFromJsonAsync<AuthorResponse>();
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Id, Is.EqualTo(created.Id));
    }

    [Test]
    public async Task GetAuthorById_Should_Return404_When_AuthorDoesNotExist()
    {
        var missingId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/authors/{missingId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetAllAuthors_Should_ReturnPagedList_WithFiltersAndPagination()
    {
        await Client.PostAsJsonAsync("/api/authors", new CreateAuthorCommand { FirstName = "Erich", LastName = "Gamma" });
        await Client.PostAsJsonAsync("/api/authors", new CreateAuthorCommand { FirstName = "Richard", LastName = "Helm" });

        var response = await Client.GetAsync("/api/authors?page=1&pageSize=10&firstName=Erich");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var paged = await response.Content.ReadFromJsonAsync<GetAllAuthorsResponse>();
        Assert.That(paged, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(paged!.TotalCount, Is.EqualTo(1));
            Assert.That(paged.Items.First().FirstName, Is.EqualTo("Erich"));
        });
    }

    [Test]
    public async Task UpdateAuthorById_Should_MutateAuthor_When_Valid()
    {
        var createResp = await Client.PostAsJsonAsync("/api/authors", new CreateAuthorCommand { FirstName = "OldFirst", LastName = "OldLast" });
        var created = await createResp.Content.ReadFromJsonAsync<AuthorResponse>();

        var updateCmd = new UpdateAuthorByIdCommand
        {
            Id = created!.Id,
            FirstName = "NewFirst",
            LastName = "NewLast"
        };

        var response = await Client.PutAsJsonAsync($"/api/authors/{created.Id}", updateCmd);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<AuthorResponse>();
        Assert.Multiple(() =>
        {
            Assert.That(updated!.FirstName, Is.EqualTo("NewFirst"));
            Assert.That(updated.LastName, Is.EqualTo("NewLast"));
        });
    }

    [Test]
    public async Task UpdateAuthorById_Should_Return404_When_NotFound()
    {
        var missingId = Guid.NewGuid();
        var updateCmd = new UpdateAuthorByIdCommand { Id = missingId, FirstName = "A", LastName = "B" };

        var response = await Client.PutAsJsonAsync($"/api/authors/{missingId}", updateCmd);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteAuthorById_Should_RemoveAuthor_AndReturn204()
    {
        var createResp = await Client.PostAsJsonAsync("/api/authors", new CreateAuthorCommand { FirstName = "Temp", LastName = "Author" });
        var created = await createResp.Content.ReadFromJsonAsync<AuthorResponse>();

        var deleteResp = await Client.DeleteAsync($"/api/authors/{created!.Id}");

        Assert.That(deleteResp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getResp = await Client.GetAsync($"/api/authors/{created.Id}");
        Assert.That(getResp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteAuthorById_Should_Return404_When_NotFound()
    {
        var missingId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"/api/authors/{missingId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
