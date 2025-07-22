namespace Server.Palaro2026.Entities;

public partial class News
{
    public string? ID { get; set; }

    public int? NewsCategoryID { get; set; }

    public string? AuthorID { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? Excerpt { get; set; }

    public DateTime? DateCreated { get; set; }

    public bool? IsPublished { get; set; }

    public DateTime? DatePublished { get; set; }

    public bool? IsArchived { get; set; }

    public virtual Users? Author { get; set; }

    public virtual NewsCategories? NewsCategory { get; set; }
}
