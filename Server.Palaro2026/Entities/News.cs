using System;
using System.Collections.Generic;

namespace Server.Palaro2026.Entities;

public partial class News
{
    public int ID { get; set; }

    public int? NewsCategoryID { get; set; }

    public string? AuthorID { get; set; }

    public string? Content { get; set; }

    public DateTime? DatePosted { get; set; }

    public virtual Users? Author { get; set; }

    public virtual NewsCategories? NewsCategory { get; set; }
}
