using AutoMapper;
using BookCatalog.Application.Handlers.Author.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class CreateAuthorCommandHandlerTests
{
    private Mock<IAuthorRepository> _authorRepository = null!;
    private IMapper _mapper = null!;
    private CreateAuthorCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _authorRepository = new Mock<IAuthorRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _sut = new CreateAuthorCommandHandler(_mapper, _authorRepository.Object);
    }

    [Test]
    public async Task Handle_Should_PersistAuthorAndReturnMappedResponse_WhenCommandIsValid()
    {
        Author? captured = null;
        _authorRepository
            .Setup(r => r.InsertAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .Callback<Author, CancellationToken>((a, _) => captured = a)
            .Returns(Task.CompletedTask);

        var command = AuthorFaker.CreateCommand();

        var response = await _sut.Handle(command, CancellationToken.None);

        Assert.That(captured, Is.Not.Null,
            "CreateAuthorCommandHandler must invoke IAuthorRepository.InsertAsync with the new author");

        Assert.Multiple(() =>
        {
            Assert.That(captured!.FirstName, Is.EqualTo(command.FirstName),
                "Inserted Author.FirstName must mirror CreateAuthorCommand.FirstName");
            Assert.That(captured.LastName, Is.EqualTo(command.LastName),
                "Inserted Author.LastName must mirror CreateAuthorCommand.LastName");
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.FirstName, Is.EqualTo(command.FirstName),
                "AuthorResponse.FirstName must reflect the persisted author's first name");
            Assert.That(response.LastName, Is.EqualTo(command.LastName),
                "AuthorResponse.LastName must reflect the persisted author's last name");
        });

        _authorRepository.Verify(
            r => r.InsertAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "CreateAuthorCommandHandler must persist the new author exactly once");
    }
}
