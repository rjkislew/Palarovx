namespace Server.Palaro2026.DTO
{
    public class Sports
    {
        public class s_CategoriesDTO
        {
            public string? category { get; set; }
        }

        public class s_LevelsDTO
        {
            public string? level { get; set; }
        }

        public class s_SportsDTO
        {
            public string? sport { get; set; }
            public string? description { get; set; }
        }

        public class s_GendersDTO
        {
            public string? gender_category { get; set; }
        }

        public class s_SubCategoriesDTO
        {
            public string? sub_category { get; set; }
        }
    }
}
