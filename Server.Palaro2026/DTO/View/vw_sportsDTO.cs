namespace Server.Palaro2026.DTO.View
{
    public class vw_categoryDTO
    {
        public string? category { get; set; }
        public List<levelDTO>? level { get; set; }
    }

    public class levelDTO
    {
        public string? level { get; set; }
        public List<gender_categoryDTO>? gender_category { get; set; }
    }

    public class gender_categoryDTO
    {
        public string? gender_category { get; set; }
        public List<sportDTO>? sport { get; set; }
    }

    public class sportDTO
    {
        public string? sport { get; set; }
        public List<sub_categoryDTO>? sub_category { get; set; }
    }

    public class sub_categoryDTO
    {
        public string? sub_category { get; set; }
    }


    //sub categories
    public class vw_category_sub_categoryDTO
    {
        public string? category { get; set; }
        public List<sportDTO_listDTO>? sports { get; set; }
    }
    public class sportDTO_listDTO
    {
        public string? sport { get; set; }
    }
}
