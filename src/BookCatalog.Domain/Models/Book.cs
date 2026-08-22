namespace BookCatalog.Domain.Models;

public class Book
{
    public Guid Id { get; set; }
    public string Isbn { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int NumberOfPages { get; set; }
    public DateTime? PublishDate { get; set; }

    public Book(string isbn, string title, string author, int numberOfPages)
    {
        Isbn = isbn;
        Title = title;
        Author = author;
        NumberOfPages = numberOfPages;
    }

    public Book(string isbn, string title, string author, int numberOfPages, DateTime? publishDate)
    {
        Isbn = isbn;
        Title = title;
        Author = author;
        NumberOfPages = numberOfPages;
        PublishDate = publishDate;
    }
}