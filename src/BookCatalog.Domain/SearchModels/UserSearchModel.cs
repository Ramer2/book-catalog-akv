using BookCatalog.Domain.Pagination;

namespace BookCatalog.Domain.SearchModels;

public record UserSearchModel : BaseSearchModelPagedQuery
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
