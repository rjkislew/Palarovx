namespace Server.Palaro2026.DTO;

public class NewsDTO
{
    public class NewsDetails
    {
        public class NewsContent
        {
            public string? ID { get; set; }
            public string? Category { get; set; }
            public string? Author { get; set; }
            public string? Title { get; set; }
            public string? Content { get; set; }
            public string? Excerpt { get; set; }
            public DateTime? DateCreated { get; set; }
            public bool? IsPublished { get; set; }
            public DateTime? DatePublished { get; set; }
            public bool? IsArchived { get; set; }
        }
    }

    public class NewsCategories
    {
        public int ID { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
    }

    public class News
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
    }
}
