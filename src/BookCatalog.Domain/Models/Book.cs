namespace BookCatalog.Domain.Models;

public class Book
{
    public Guid Id { get; set; }
    public string Isbn { get; set; }
    public string Title { get; set; }
    public Guid AuthorId { get; set; }
    public Author? Author { get; set; }
    public int NumberOfPages { get; set; }
    public DateTime? PublishDate { get; set; }

    public Book(string isbn, string title, Guid authorId, int numberOfPages)
    {
        Isbn = isbn;
        Title = title;
        AuthorId = authorId;
        NumberOfPages = numberOfPages;
    }

    public Book(string isbn, string title, Guid authorId, int numberOfPages, DateTime? publishDate)
    {
        Isbn = isbn;
        Title = title;
        AuthorId = authorId;
        NumberOfPages = numberOfPages;
        PublishDate = publishDate;
    }
}
