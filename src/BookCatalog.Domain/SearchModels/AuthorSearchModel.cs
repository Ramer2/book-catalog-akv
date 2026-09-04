using BookCatalog.Domain.Pagination;

namespace BookCatalog.Domain.SearchModels;

public record AuthorSearchModel : BaseSearchModelPagedQuery
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
