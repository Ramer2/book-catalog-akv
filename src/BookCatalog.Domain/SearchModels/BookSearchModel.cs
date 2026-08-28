using BookCatalog.Domain.Pagination;

namespace BookCatalog.Domain.SearchModels;

public record BookSearchModel : BaseSearchModelPagedQuery
{
    public string? Isbn { get; init; }
    public string? Title { get; set; }
    public string? Author { get; set; }
}