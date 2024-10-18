namespace Server.Palaro2026.DTO
{
    public class SportsDTO
    {
        public class s_CategoriesDTO
        {
            public string? Category { get; set; }
        }

        public class s_LevelsDTO
        {
            public string? Level { get; set; }
        }

        public class s_SportsDTO
        {
            public string? Sport { get; set; }
            public string? Description { get; set; }
        }

        public class s_GendersDTO
        {
            public string? GenderCategory { get; set; }
        }

        public class s_SubCategoriesDTO
        {
            public string? SubCategory { get; set; }
        }
    }
}
