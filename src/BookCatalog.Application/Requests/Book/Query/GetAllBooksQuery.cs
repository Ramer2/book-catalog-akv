using BookCatalog.Application.Responses.Book;
using BookCatalog.Domain.SearchModels;

namespace BookCatalog.Application.Requests.Book.Query;

public record GetAllBooksQuery : BookSearchModel, IQuery<GetAllBooksResponse>
{
}