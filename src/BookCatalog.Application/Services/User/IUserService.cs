namespace BookCatalog.Application.Services.User;

public interface IUserService
{
    public Task<Domain.Models.User> GetOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<bool> EnsureEmailUniqueAsync(
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default);

    public Task<bool> EnsurePhoneNumberUniqueAsync(
        string phoneNumber,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default);
}
