using BookCatalog.Application.Responses.User;

namespace BookCatalog.Application.Requests.User.Query;

public class GetUserByIdQuery : IQuery<UserResponse>
{
    public Guid Id { get; set; }
}
