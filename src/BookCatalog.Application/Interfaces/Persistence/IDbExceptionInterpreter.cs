namespace BookCatalog.Application.Interfaces.Persistence;

public interface IDbExceptionInterpreter
{
    bool IsUniqueViolation(Exception exception, string? indexName = null);
}
