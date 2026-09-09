using AutoMapper;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.User.Query;
using BookCatalog.Application.Responses.User;
using MediatR;

namespace BookCatalog.Application.Handlers.User.Query;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, GetAllUsersResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<GetAllUsersResponse> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(request, cancellationToken);
        return new GetAllUsersResponse
        {
            Items = _mapper.Map<IEnumerable<UserResponse>>(users.Items),
            TotalCount = users.TotalCount,
            TotalPages = users.TotalPages,
            Page = users.Page,
            PageSize = users.PageSize
        };
    }
}
