using System.Linq.Expressions;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
using BookCatalog.Domain.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BookCatalogDbContext _dbContext;

    public UserRepository(BookCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static readonly IReadOnlyDictionary<string, Expression<Func<User, object>>> SortColumns =
        new Dictionary<string, Expression<Func<User, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(User.Email)] = u => u.Email,
            [nameof(User.FirstName)] = u => u.FirstName,
            [nameof(User.LastName)] = u => u.LastName,
            [nameof(User.BirthDate)] = u => u.BirthDate,
            [nameof(User.CreatedAt)] = u => u.CreatedAt
        };

    public async Task<BaseSearchModelPagedResponse<User>> GetAllAsync(UserSearchModel request,
        CancellationToken cancellationToken)
    {
        var filtered = ApplyFilters(_dbContext.Users, request);
        var totalCount = await filtered.CountAsync(cancellationToken);

        var ordered = ApplySorting(filtered, request);
        var users = await ApplyPaging(ordered, request).ToListAsync(cancellationToken);

        return BuildResponse(users, totalCount, request);
    }

    private static IQueryable<User> ApplyFilters(IQueryable<User> query, UserSearchModel request)
    {
        if (!string.IsNullOrEmpty(request.Email))
            query = query.Where(u => u.Email.ToLower().Contains(request.Email.ToLower()));

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            query = query.Where(u => u.PhoneNumber.ToLower().Contains(request.PhoneNumber.ToLower()));

        if (!string.IsNullOrEmpty(request.FirstName))
            query = query.Where(u => u.FirstName.ToLower().Contains(request.FirstName.ToLower()));

        if (!string.IsNullOrEmpty(request.LastName))
            query = query.Where(u => u.LastName.ToLower().Contains(request.LastName.ToLower()));

        return query;
    }

    private static IOrderedQueryable<User> ApplySorting(IQueryable<User> query, UserSearchModel request)
    {
        if (!string.IsNullOrWhiteSpace(request.SortBy)
            && SortColumns.TryGetValue(request.SortBy, out var lambda))
        {
            return request.Desc
                ? query.OrderByDescending(lambda).ThenBy(u => u.Id)
                : query.OrderBy(lambda).ThenBy(u => u.Id);
        }

        return query.OrderBy(u => u.Id);
    }

    private static IQueryable<User> ApplyPaging(IQueryable<User> query, UserSearchModel request)
    {
        var (page, pageSize) = SanitizePaging(request);
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }

    private static BaseSearchModelPagedResponse<User> BuildResponse(
        IReadOnlyList<User> items, int totalCount, UserSearchModel request)
    {
        var (page, pageSize) = SanitizePaging(request);
        var totalPages = totalCount == 0 ? 0 : (totalCount + pageSize - 1) / pageSize;

        return new BaseSearchModelPagedResponse<User>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page
        };
    }

    private static (int Page, int PageSize) SanitizePaging(UserSearchModel request)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 1 : request.PageSize;
        return (page, pageSize);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task InsertAsync(User entity, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(entity, cancellationToken);
        await SaveAsync(cancellationToken);
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user != null)
        {
            _dbContext.Users.Remove(user);
            await SaveAsync(cancellationToken);
        }
    }

    public Task DeleteEntityAsync(User entity, CancellationToken cancellationToken)
    {
        _dbContext.Users.Remove(entity);
        return SaveAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
