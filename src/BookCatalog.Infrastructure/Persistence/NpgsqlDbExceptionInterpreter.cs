using BookCatalog.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookCatalog.Infrastructure.Persistence;

/// <summary>
/// Npgsql-specific implementation. Postgres reports unique constraint
/// violations with SQLSTATE 23505 (see https://www.postgresql.org/docs/current/errcodes-appendix.html).
/// </summary>
public class NpgsqlDbExceptionInterpreter : IDbExceptionInterpreter
{
    private const string UniqueViolationSqlState = "23505";

    public bool IsUniqueViolation(Exception exception, string? indexName = null)
    {
        var postgres = FindPostgresException(exception);
        if (postgres is null || postgres.SqlState != UniqueViolationSqlState)
            return false;

        if (indexName is null)
            return true;

        // Postgres surfaces the offending index/constraint via ConstraintName.
        return string.Equals(postgres.ConstraintName, indexName, StringComparison.Ordinal);
    }

    private static PostgresException? FindPostgresException(Exception exception)
    {
        // EF Core wraps the driver exception in DbUpdateException; unwrap it.
        Exception? current = exception is DbUpdateException dbEx ? dbEx.InnerException : exception;
        while (current is not null)
        {
            if (current is PostgresException pg)
                return pg;
            current = current.InnerException;
        }
        return null;
    }
}
