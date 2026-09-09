using AutoMapper;
using BookCatalog.Application.Requests.User.Query;
using BookCatalog.Application.Responses.User;
using BookCatalog.Application.Services.User;
using MediatR;

namespace BookCatalog.Application.Handlers.User.Query;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IMapper mapper, IUserService userService)
    {
        _mapper = mapper;
        _userService = userService;
    }

    public async Task<UserResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetOrThrowAsync(request.Id, cancellationToken);
        return _mapper.Map<UserResponse>(user);
    }
}
