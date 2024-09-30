namespace Server.Palaro2026.DTO.View
{
    public class vw_regional_teamsDTO
    {
        public string? regional_team_name { get; set; }
        public string? regional_team_name_abbreviation { get; set; }
        public List<division_nameDTO>? division_name { get; set; }
    }

    public class division_nameDTO
    {
        public string? division_name { get; set; }
        public List<school_nameDTO>? school_name { get; set; }

    }

    public class school_nameDTO
    {
        public string? school_name { get; set; }

    }
}
