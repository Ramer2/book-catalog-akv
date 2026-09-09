using BookCatalog.Domain.Models;
using BookCatalog.Domain.SearchModels;

namespace BookCatalog.Application.Interfaces.Repositories;

public interface IAuthorRepository : IRepository<Author, AuthorSearchModel>
{
}
