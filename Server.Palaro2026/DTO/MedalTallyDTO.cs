namespace Server.Palaro2026.DTO
{
    public class MedalTallyDTO
    {
        public class RegionalMedalTally
        {
            public string? Region { get; set; }
            public string? Abbreviation { get; set; }
            public int? Gold { get; set; }
            public int? Silver { get; set; }
            public int? Bronze { get; set; }
            public int? Total { get; set; }
        }
    }
}
