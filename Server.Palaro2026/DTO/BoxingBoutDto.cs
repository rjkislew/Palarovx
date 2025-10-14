namespace Server.Palaro2026.DTO
{
   

    public class CreateBoxingBoutDto
    {
        public int RoundId { get; set; } // Add this
        public string Round { get; set; } = string.Empty;
        public string RedCornerRegion { get; set; } = string.Empty;
        public string BlueCornerRegion { get; set; } = string.Empty;
        public DateTime? Schedule { get; set; }
        public string Arena { get; set; } = string.Empty;
        public string Referee { get; set; } = string.Empty;
        public int MajorCategoryId { get; set; }
        public int CategoryId { get; set; }
    }

    public class UpdateBoxingBoutDto
    {
        public DateTime? Schedule { get; set; }
        public string Arena { get; set; } = string.Empty;
        public string Referee { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class ScoreBoxingBoutDto
    {
        public string WinnerRegion { get; set; } = string.Empty;
        public string VictoryType { get; set; } = string.Empty;
        public int RoundStopped { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class RegionBoutDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class ArenaDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class RefereeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class MajorCategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int LevelId { get; set; }
    }
    public class RoundsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
    }
    public class ParticipantsDto
    {
        public string Id { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public int SportSubcategoryID { get; set; }
        public string CoachName { get; set; } = string.Empty;
        public int SchoolRegionID { get; set; }
        public string Region { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }


    public class BoutDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }


    public class CreateBoxingMatchDto
    {
        public int RoundId { get; set; }
        public string Round { get; set; } = string.Empty;
        public int BoutId { get; set; }
        public ParticipantDetailsDto RedCorner { get; set; } = new();
        public ParticipantDetailsDto BlueCorner { get; set; } = new();
        public DateTime? Schedule { get; set; }
        public string Arena { get; set; } = string.Empty; // Arena name
        public string Referee { get; set; } = string.Empty;
        public int MajorCategoryId { get; set; }
        public int CategoryId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class ParticipantDetailsDto
    {
        public string ParticipantId { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public int RegionId { get; set; }
        public string Abbreviation { get; set; } = string.Empty;
        public string CoachName { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    // Add these DTO classes to your @code block
    public class EventDrawOfMatchDTO
    {
        public int Id { get; set; }
        public int Sport_Id { get; set; }
        public int MajorCategories_Id { get; set; }
        public string MajorCategory { get; set; } = string.Empty;
        public int Categories_Id { get; set; }
        public string SubCategory { get; set; } = string.Empty;
        public int Bout_Id { get; set; }
        public int Round_Id { get; set; }
        public string Round_Name { get; set; } = string.Empty;
        public int Arena_Id { get; set; }
        public string Arena_Name { get; set; } = string.Empty;
        public DateTime Schedule { get; set; }
        public string User_Id { get; set; } = string.Empty;
        public DateTime Datentime { get; set; }
        public List<EventDrawOfMatchParticipantsDTO> Participants { get; set; } = new();
    }

    public class EventDrawOfMatchParticipantsDTO
    {
        public int Id { get; set; }
        public int Match_Id { get; set; }
        public int Side_Id { get; set; }
        public string Side_Name { get; set; } = string.Empty;
        public int Region_Id { get; set; }
        public string Participant_Id { get; set; } = string.Empty;
        public string Participant_Name { get; set; } = string.Empty;
        public string Region_Name { get; set; } = string.Empty;
        public string Region_Abbr { get; set; } = string.Empty;
        public string Coach_Name { get; set; } = string.Empty;
    }

    // Update your BoxingBoutDto to match API structure
    public class BoxingBoutDto
    {
        public int Id { get; set; }
        public int Bout_Id { get; set; }
        public string MajorCategory { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Round { get; set; } = string.Empty;
        public string Round_Name { get; set; } = string.Empty;
        public string Arena_Name { get; set; } = string.Empty;
        public DateTime Schedule { get; set; }
        public string Status { get; set; } = "Scheduled"; // Default status
        public string RedCornerRegion { get; set; } = string.Empty;
        public string RedCornerName { get; set; } = string.Empty;
        public string RedCornerCoach { get; set; } = string.Empty;
        public string BlueCornerRegion { get; set; } = string.Empty;
        public string BlueCornerName { get; set; } = string.Empty;
        public string BlueCornerCoach { get; set; } = string.Empty;
        public string WinnerRegion { get; set; } = string.Empty;
        public string VictoryType { get; set; } = string.Empty;
        public string Referee { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }


}