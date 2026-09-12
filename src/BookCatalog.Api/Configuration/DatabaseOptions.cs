using System.ComponentModel.DataAnnotations;

namespace BookCatalog.Api.Configuration;

public class DatabaseOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Database ConnectionString is required.")]
    public string ConnectionString { get; set; } = string.Empty;
}
