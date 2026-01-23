namespace Server.Palaro2026.DTO
{
    public class ProgressionDTO
    {
        public class ProgressionTeamSelection
        {
            public int? SelectedRegionID { get; set; }
            public List<string> SelectedPlayerIDs { get; set; } = new();
            public List<ProfilePlayerDTO> AvailablePlayers { get; set; } = new();
            public int ExistingVersusTeamID { get; set; }
            public string? Score { get; set; }
            public string? Rank { get; set; }
        }

        public class ProfilePlayerDTO
        {
            public string? ID { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
        }
    }
}
