using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Exceptions;

namespace BookCatalog.Application.Services.User;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Domain.Models.User> GetOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new EntityNotFoundException("No user found for a given id.");
        return user;
    }

    public async Task<bool> EnsureEmailUniqueAsync(
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existing is null)
            return true;
        return excludeUserId.HasValue && existing.Id == excludeUserId.Value;
    }

    public async Task<bool> EnsurePhoneNumberUniqueAsync(
        string phoneNumber,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (existing is null)
            return true;
        return excludeUserId.HasValue && existing.Id == excludeUserId.Value;
    }
}
