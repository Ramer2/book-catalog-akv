using BookCatalog.Domain.Pagination;

namespace BookCatalog.Domain.SearchModels;

public record LoanSearchModel : BaseSearchModelPagedQuery
{
    public Guid? UserId { get; init; }
    public Guid? BookId { get; init; }
    public bool? IsReturned { get; init; }
}
