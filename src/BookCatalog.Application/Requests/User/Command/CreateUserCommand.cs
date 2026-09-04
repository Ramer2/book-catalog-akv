using BookCatalog.Application.Responses.User;

namespace BookCatalog.Application.Requests.User.Command;

public class CreateUserCommand : ICommand<UserResponse>
{
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly BirthDate { get; set; }
}
