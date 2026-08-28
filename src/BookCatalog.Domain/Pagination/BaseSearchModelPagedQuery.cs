namespace BookCatalog.Domain.Pagination;

public abstract record BaseSearchModelPagedQuery
{
    public string? SortBy { get; set; }
    public bool Desc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}