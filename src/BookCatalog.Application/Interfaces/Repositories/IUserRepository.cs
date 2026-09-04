using BookCatalog.Domain.Models;
using BookCatalog.Domain.SearchModels;

namespace BookCatalog.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User, UserSearchModel>
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    public Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
}
