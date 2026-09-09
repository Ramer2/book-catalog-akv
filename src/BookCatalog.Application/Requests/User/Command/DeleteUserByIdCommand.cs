namespace BookCatalog.Application.Requests.User.Command;

public class DeleteUserByIdCommand : ICommand
{
    public Guid Id { get; set; }
}
