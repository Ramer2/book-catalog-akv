namespace BookCatalog.Domain.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public User(string email, string phoneNumber, string firstName, string lastName, DateOnly birthDate)
    {
        Email = email;
        PhoneNumber = phoneNumber;
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        CreatedAt = DateTime.UtcNow;
    }
}
