using BookCatalog.Application.Responses.User;
using BookCatalog.Domain.SearchModels;

namespace BookCatalog.Application.Requests.User.Query;

public record GetAllUsersQuery : UserSearchModel, IQuery<GetAllUsersResponse>
{
}
