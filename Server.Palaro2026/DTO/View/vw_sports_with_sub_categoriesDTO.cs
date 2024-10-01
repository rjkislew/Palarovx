namespace Server.Palaro2026.DTO.View
{
    public class vw_sports_with_sub_categoriesDTO
    {
        public string? category { get; set; }
        public List<sport_list_with_sub_categoriesDTO>? sports { get; set; }
    }

    public class sport_list_with_sub_categoriesDTO
    {
        public string? sport { get; set; }
        public string? description { get; set; }
        public List<sub_category_with_sub_categoriesDTO>? sub_categories { get; set; }
    }

    public class sub_category_with_sub_categoriesDTO
    {
        public string? sub_category { get; set; }
    }
}
