namespace Server.Palaro2026.DTO
{
    public class EventsDTO
    {
        public class EventDetail
        {
            public class ED_DateContent
            {
                public DateTime? Date { get; set; }
                public List<ED_SportCategoriesContent>? CategoryList { get; set; }
            }

            public class ED_SportCategoriesContent
            {
                public string? Category { get; set; }
                public List<ED_SportsContent>? SportList { get; set; }
            }

            public class ED_SportsContent
            {
                public string? Sport { get; set; }
                public List<ED_SchoolLevelsContent>? LevelList { get; set; }
            }

            public class ED_SchoolLevelsContent
            {
                public string? Level { get; set; }
                public List<ED_GenderCategoriesContent>? GenderList { get; set; }
            }

            public class ED_GenderCategoriesContent
            {
                public string? Gender { get; set; }
                public List<ED_SubCategoriesContent>? SportSubcategoryList { get; set; }
            }

            public class ED_SubCategoriesContent
            {
                public string? Subcategory { get; set; }
                public List<ED_EventsContent>? EventList { get; set; }
            }

            public class ED_EventsContent
            {
                public string ID { get; set; } = null!;
                public string? Venue { get; set; }
                public DateTime? Date { get; set; }
                public TimeSpan? Time { get; set; }
                public bool? OnStream { get; set; }
                public string? StreamURL { get; set; }
                public bool? IsFinished { get; set; }
                public byte[]? Attachement { get; set; }
                public bool? Archived { get; set; }
                public bool? Deleted { get; set; }
                public List<ED_RegionsContent>? TeamList { get; set; }
            }

            public class ED_RegionsContent
            {
                public string? Region { get; set; }
                public string? Abbreviation { get; set; }
                public int? Score { get; set; }
            }
        }

        public class Events
        {
            public class EventsContent
            {
                public string? ID { get; set; }
                public int? SportSubcategoryID { get; set; }
                public int? VenueID { get; set; }
                public DateTime? Date { get; set; }
                public TimeSpan? Time { get; set; }
                public bool? OnStream { get; set; }
                public string? StreamURL { get; set; }
                public bool? IsFinished { get; set; }
                public byte[]? Attachement { get; set; }
                public bool? Archived { get; set; }
                public bool? Deleted { get; set; }
            }
        }

        public class EventVersus
        {
            public class EventVersusContent
            {
                public int ID { get; set; }
                public int? Score { get; set; }
                public int? RegionID { get; set; }
                public string? EventID { get; set; }
            }
        }
    }
}