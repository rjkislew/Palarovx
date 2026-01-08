using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Services.UploadServices;

public class CoachImportService : ICoachImportService
{
    private readonly Palaro2026Context _db;
    private readonly ILogger<CoachImportService> _logger;
    private const int BatchSize = 200;

    private const int BOYS_GENDER_ID = 1;
    private const int GIRLS_GENDER_ID = 2;
    private const int MIXED_GENDER_ID = 3;

    public CoachImportService(Palaro2026Context db, ILogger<CoachImportService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ImportResult> ImportCoachesFromExcelAsync(Stream excelStream, string uploadedBy = "system")
    {
        var result = new ImportResult();

        using var package = new ExcelPackage(excelStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet?.Dimension == null)
        {
            result.Errors.Add("No worksheet found or worksheet is empty.");
            return result;
        }

        // Header detection - adjusted for your Excel structure
        int headerRow = DetectHeaderRow(worksheet);
        if (headerRow == -1)
        {
            result.Errors.Add("Header row not detected.");
            return result;
        }

        var headerMap = BuildHeaderMap(worksheet, headerRow);

        // Get column indexes - UPDATED for your Excel columns
        var colMapping = GetColumnMappings(headerMap);
        if (colMapping.LastName == -1 || colMapping.FirstName == -1)
        {
            result.Errors.Add("Could not find 'Last Name' or 'First Name' columns.");
            return result;
        }

        var sportsList = await _db.Sports
            .AsNoTracking()
            .Where(s => !string.IsNullOrEmpty(s.Sport))
            .Select(s => new { s.ID, s.Sport, s.SportCategoryID })
            .ToListAsync();

        // UPDATED: Pre-load lookup data with normalized sport names (same as player import)
        var sportsCache = await _db.Sports
            .AsNoTracking()
            .Where(s => !string.IsNullOrEmpty(s.Sport))
            .ToDictionaryAsync(
                s => $"{NormalizeSportName(s.Sport.Trim())}|{GetCategoryName(s.SportCategoryID)}",
                s => s.ID,
                StringComparer.OrdinalIgnoreCase
            );

        // Pre-load regions for lookup
        var regionsCache = await _db.SchoolRegions
            .AsNoTracking()
            .ToDictionaryAsync(
                r => r.Region.Trim().ToLower(),
                r => r.ID
            );

        var schoolsList = await _db.Schools.AsNoTracking().ToListAsync();
        var schoolCacheByName = schoolsList
            .GroupBy(s => s.School?.Trim().ToLower() ?? "")
            .ToDictionary(
                g => g.Key,
                g => g.First(), // Take the first school when duplicates exist
                StringComparer.OrdinalIgnoreCase
            );

        var coachesBatch = new List<ProfileCoaches>();
        int startRow = headerRow + 1;
        int endRow = worksheet.Dimension.End.Row;
        result.TotalRows = Math.Max(0, endRow - headerRow);

        for (int r = startRow; r <= endRow; r++)
        {
            try
            {
                if (!RowHasData(worksheet, r, worksheet.Dimension.End.Column))
                {
                    result.SkippedCount++;
                    continue;
                }

                string lastName = GetCellValue(worksheet, r, colMapping.LastName);
                string firstName = GetCellValue(worksheet, r, colMapping.FirstName);

                if (string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(firstName))
                {
                    result.SkippedCount++;
                    continue;
                }

                // Process the row
                var coach = await ProcessCoachRow(worksheet, r, colMapping, sportsCache, regionsCache,
                    schoolCacheByName, uploadedBy);
                if (coach != null)
                {
                    coachesBatch.Add(coach);
                }

                if (coachesBatch.Count >= BatchSize)
                {
                    await PersistBatchAsync(coachesBatch);
                    coachesBatch.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing row {Row}", r);
                result.Errors.Add($"Row {r}: {ex.Message}");
                result.SkippedCount++;
            }
        }

        // Final batch
        if (coachesBatch.Any())
        {
            await PersistBatchAsync(coachesBatch);
        }

        result.ImportedCount = result.TotalRows - result.SkippedCount - result.Errors.Count;
        return result;
    }

    // NEW: Method to normalize sport names from Excel (same as player import)
    private string NormalizeSportName(string sportName)
    {
        if (string.IsNullOrWhiteSpace(sportName))
            return sportName;

        string normalized = sportName.Trim().ToUpperInvariant();

        // Remove variations and keep only the base sport name
        if (normalized.Contains("BASKETBALL"))
        {
            return "BASKETBALL"; // Always map to BASKETBALL regardless of variations
        }
        else if (normalized.Contains("DANCE SPORTS") || normalized.Contains("DANCESPORT") ||
                 normalized.Contains("DANCE"))
        {
            return "DANCE SPORTS"; // Always map to DANCE SPORTS
        }
        else if (normalized.Contains("GYMNASTICS"))
        {
            return "GYMNASTICS"; // Always map to GYMNASTICS regardless of variations
        }
        else if (normalized.Contains("VOLLEYBALL"))
        {
            return "VOLLEYBALL"; // Always map to VOLLEYBALL
        }
        else if (normalized.Contains("FOOTBALL"))
        {
            return "FOOTBALL"; // Always map to FOOTBALL
        }
        else if (normalized.Contains("SWIMMING"))
        {
            return "SWIMMING"; // Always map to SWIMMING
        }
        else if (normalized.Contains("ATHLETICS") || normalized.Contains("TRACK AND FIELD"))
        {
            return "ATHLETICS"; // Always map to ATHLETICS
        }
        else if (normalized.Contains("TABLE TENNIS"))
        {
            return "TABLE TENNIS"; // Always map to TABLE TENNIS
        }
        else if (normalized.Contains("BADMINTON"))
        {
            return "BADMINTON"; // Always map to BADMINTON
        }
        else if (normalized.Contains("CHESS"))
        {
            return "CHESS"; // Always map to CHESS
        }
        else if (normalized.Contains("FUTZAL"))
        {
            return "FUTSAL"; // Always map to CHESS
        }

        // For other sports, return as is but remove any parenthetical content
        var index = normalized.IndexOf('(');
        if (index > 0)
        {
            normalized = normalized.Substring(0, index).Trim();
        }

        return normalized;
    }

    // NEW: Category name mapping (same as player import)
    private string GetCategoryName(int? sportCategoryId)
    {
        if (!sportCategoryId.HasValue) return "Regular Sports";

        return sportCategoryId switch
        {
            1 => "Regular Sports",
            2 => "Demo Sports",
            3 => "Paragames",
            _ => "Regular Sports"
        };
    }

    private int DetectHeaderRow(ExcelWorksheet worksheet)
    {
        var maxHeaderScan = Math.Min(40, worksheet.Dimension.End.Row);
        for (int r = 1; r <= maxHeaderScan; r++)
        {
            var rowValues = new List<string>();
            for (int c = 1; c <= worksheet.Dimension.End.Column; c++)
            {
                rowValues.Add((worksheet.Cells[r, c].Text ?? "").Trim().ToLowerInvariant());
            }

            // Look for coach-specific headers based on your Excel structure
            if (rowValues.Any(v => v.Contains("lastname") || v.Contains("last name")) &&
                rowValues.Any(v => v.Contains("firstname") || v.Contains("first name")) &&
                rowValues.Any(v => v.Contains("designation")) &&
                rowValues.Any(v => v.Contains("event") || v.Contains("sport")))
            {
                return r;
            }
        }

        return -1;
    }

    private Dictionary<string, int> BuildHeaderMap(ExcelWorksheet worksheet, int headerRow)
    {
        var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int c = 1; c <= worksheet.Dimension.End.Column; c++)
        {
            var headerText = (worksheet.Cells[headerRow, c].Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(headerText))
            {
                headerMap[headerText] = c;
            }
        }

        return headerMap;
    }

    // UPDATED: Column mappings to match your Excel file structure
    private (int LastName, int FirstName, int MI, int Sex, int BirthDate, int Designation, int Sport, int GenderCategory
        ,
        int SchoolName, int SchoolLevel, int SchoolDivision, int Region)
        GetColumnMappings(Dictionary<string, int> headerMap)
    {
        int GetCol(params string[] names)
        {
            foreach (var n in names)
            {
                var match = headerMap.Keys.FirstOrDefault(k => string.Equals(k, n, StringComparison.OrdinalIgnoreCase));
                if (match != null) return headerMap[match];

                foreach (var key in headerMap.Keys)
                {
                    if (key.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0)
                        return headerMap[key];
                }
            }

            return -1;
        }

        return (
            GetCol("lastname", "last name", "last"),
            GetCol("firstname", "first name", "first"),
            GetCol("midname", "middle name", "mi", "middle"),
            GetCol("sex", "gender"),
            GetCol("bdate", "birthdate", "birth date", "birth"),
            GetCol("designation", "position", "role"),
            GetCol("event", "sport", "sports"), // Your Excel uses "EVENT" column
            GetCol("b_or_g", "gender category", "category", "gender"), // Your Excel uses "B_or_G" column
            GetCol("schoolname", "school", "schoolname"),
            GetCol("level", "school level", "schoollevel"), // Your Excel uses "LEVEL" column
            GetCol("school division", "division", "schooldivision"), // Your Excel uses "School Division" column
            GetCol("region") // Added region column mapping
        );
    }

    private bool RowHasData(ExcelWorksheet worksheet, int row, int lastCol)
    {
        for (int c = 1; c <= lastCol; c++)
        {
            if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, c].Text))
                return true;
        }

        return false;
    }

    private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
    {
        return col != -1 ? (worksheet.Cells[row, col].Text ?? "").Trim() : "";
    }

    private async Task<ProfileCoaches?> ProcessCoachRow(ExcelWorksheet worksheet, int row,
    (int LastName, int FirstName, int MI, int Sex, int BirthDate, int Designation, int Sport, int GenderCategory,
        int SchoolName, int SchoolLevel, int SchoolDivision, int Region) cols,
    Dictionary<string, int> sportsCache,
    Dictionary<string, int> regionsCache,
    Dictionary<string, Schools> schoolCacheByName,
    string uploadedBy)
{
    string lastName = GetCellValue(worksheet, row, cols.LastName);
    string firstName = GetCellValue(worksheet, row, cols.FirstName);

    // Parse birthdate
    DateTime? birthDate = null;
    if (cols.BirthDate != -1)
    {
        var btext = GetCellValue(worksheet, row, cols.BirthDate);
        if (!string.IsNullOrWhiteSpace(btext))
        {
            if (DateTime.TryParse(btext, out var dt))
                birthDate = dt;
            else if (double.TryParse(btext, out var oa))
                birthDate = DateTime.FromOADate(oa);
        }
    }

    string level = GetCellValue(worksheet, row, cols.SchoolLevel);
    string sportName = GetCellValue(worksheet, row, cols.Sport);
    string category = "Regular Sports"; 
    int? sportCategoryId = 1;

    // NEW: Check for specific sports that should override level and category
    if (!string.IsNullOrWhiteSpace(sportName))
    {
        string normalizedSport = sportName.Trim().ToUpperInvariant();
        
        // If sport is PENCAK SILAT or WEIGHTLIFTING, override level and category
        if (normalizedSport.Contains("PENCAK SILAT") || normalizedSport.Contains("WEIGHTLIFTING"))
        {
            _logger.LogInformation("Overriding level and category for sport: {Sport}. Setting Level=2 (Secondary), Category=2 (Demo Sports)", sportName);
            level = "SECONDARY"; // Force level to Secondary
            category = "Demo Sports";
            sportCategoryId = 2; // Demo Sports ID
        }
    }

    // Existing level-based category assignment (only if not overridden above)
    if (sportCategoryId == 1 && !string.IsNullOrWhiteSpace(level))
    {
        if (level.Contains("PARAGAMES", StringComparison.OrdinalIgnoreCase))
        {
            category = "Paragames";
            sportCategoryId = 3;
        }
        else if (level.Contains("DEMO SPORTS", StringComparison.OrdinalIgnoreCase))
        {
            category = "Demo Sports";
            sportCategoryId = 2;
        }
    }

    // Resolve SportID with normalized names and category-based search
    int? sportId = null;
    if (cols.Sport != -1)
    {
        if (!string.IsNullOrWhiteSpace(sportName))
        {
            var normalizedSportName = NormalizeSportName(sportName);
            var searchKeys = new List<string>();

            var exactKey = $"{normalizedSportName}|{category.Trim()}";
            searchKeys.Add(exactKey);

            if (category.Equals("Paragames", StringComparison.OrdinalIgnoreCase))
            {
                searchKeys.Add($"{normalizedSportName}|Regular Sports");
            }

            foreach (var searchKey in searchKeys)
            {
                if (sportsCache.TryGetValue(searchKey, out var sid))
                {
                    sportId = sid;
                    break;
                }
            }
        }
    }

    // Resolve GenderCategoryID
    int? genderCategoryId = ResolveGenderCategoryId(worksheet, row, cols.GenderCategory);

    // Get school division ID before resolving school
    string schoolDivisionName = GetCellValue(worksheet, row, cols.SchoolDivision);
    int? schoolDivisionId = await ResolveSchoolDivisionId(schoolDivisionName);

    // Resolve SchoolID - pass the potentially overridden level
    int? schoolId = await ResolveSchoolId(worksheet, row,
        (cols.SchoolName, cols.SchoolLevel, cols.SchoolDivision, cols.Region),
        regionsCache, schoolCacheByName, level); // Pass the level parameter

    // Generate coach ID
    string coachId = GenerateCoachId(firstName, lastName, sportId);

    return new ProfileCoaches
    {
        ID = coachId,
        FirstName = firstName,
        LastName = lastName,
        MiddleInitial = GetCellValue(worksheet, row, cols.MI),
        Sex = GetCellValue(worksheet, row, cols.Sex),
        BirthDate = birthDate,
        Designation = GetCellValue(worksheet, row, cols.Designation),
        SportID = sportId,
        GenderCategoryID = genderCategoryId,
        SchoolID = schoolId,
        SchoolRegionID = await ResolveSchoolRegionId(GetCellValue(worksheet, row, cols.Region)),
        SchoolDivisionID = schoolDivisionId,
        SportCategoryID = sportCategoryId,
        UploadedBy = uploadedBy
    };
}

    private int? ResolveGenderCategoryId(ExcelWorksheet worksheet, int row, int genderCategoryCol)
    {
        if (genderCategoryCol == -1) return null;

        var genderCategory = GetCellValue(worksheet, row, genderCategoryCol);
        if (string.IsNullOrWhiteSpace(genderCategory)) return null;

        return genderCategory.Trim().ToUpper() switch
        {
            "B" or "BOYS" or "M" or "MALE" => BOYS_GENDER_ID,
            "G" or "GIRLS" or "F" or "FEMALE" => GIRLS_GENDER_ID,
            "B/G" or "MIXED" or "COED" or "BOTH" => MIXED_GENDER_ID,
            _ => null
        };
    }

    private string GenerateCoachId(string firstName, string lastName, int? sportId)
    {
        // Use same approach as player import
        if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName) && sportId.HasValue)
        {
            var namePart =
                $"{firstName.Substring(0, Math.Min(3, firstName.Length))}{lastName.Substring(0, Math.Min(3, lastName.Length))}";
            return $"C{namePart.ToUpper()}{sportId.Value}{DateTime.Now:HHmmss}";
        }

        // Fallback to GUID-based approach
        return $"C{Guid.NewGuid().ToString("N")[..19]}";
    }

    private async Task<int?> ResolveSchoolId(ExcelWorksheet worksheet, int row,
        (int SchoolName, int SchoolLevel, int SchoolDivision, int Region) cols,
        Dictionary<string, int> regionsCache,
        Dictionary<string, Schools> schoolCacheByName,
        string level = null) // Add optional level parameter
    {
        string schoolName = GetCellValue(worksheet, row, cols.SchoolName);
        string schoolLevel = level ?? GetCellValue(worksheet, row, cols.SchoolLevel); // Use overridden level if provided
        string schoolDivision = GetCellValue(worksheet, row, cols.SchoolDivision);
        string regionName = GetCellValue(worksheet, row, cols.Region);

        if (string.IsNullOrWhiteSpace(schoolName))
            return null;

        // Map school levels directly to IDs
        int? mappedSchoolLevelId = MapSchoolLevelToId(schoolLevel);

        // Resolve region ID
        int? regionId = await ResolveSchoolRegionId(regionName);

        // Resolve division ID
        int? schoolDivisionId = await ResolveSchoolDivisionId(schoolDivision);

        // Try to find existing school by name (case insensitive)
        var normalizedSchoolName = schoolName.Trim().ToLower();
        if (schoolCacheByName.TryGetValue(normalizedSchoolName, out var schoolByName))
        {
            // Check if the found school has the same level we need
            if (schoolByName.SchoolLevelsID == mappedSchoolLevelId)
            {
                return schoolByName.ID;
            }
        }

        // Create new school if needed
        var newSchool = new Schools
        {
            School = schoolName.Trim(),
            SchoolLevelsID = mappedSchoolLevelId,
            SchoolDivisionID = schoolDivisionId,
            SchoolRegionID = regionId,
            SchoolType = "Public"
        };

        _db.Schools.Add(newSchool);
        await _db.SaveChangesAsync();

        // Update cache
        schoolCacheByName[newSchool.School.ToLower()] = newSchool;

        return newSchool.ID;
    }

// NEW: School level mapping method with exact requirements (same as player import)
    private int? MapSchoolLevelToId(string originalLevel)
    {
        if (string.IsNullOrWhiteSpace(originalLevel))
        {
            _logger.LogWarning("School level is empty, defaulting to Secondary Level (ID: 2)");
            return 2; // Default to Secondary Level ID
        }

        string level = originalLevel.Trim().ToUpperInvariant();

        // Handle PARAGAMES specifically - map to ID 3
        if (level.Contains("PARAGAMES"))
        {
            _logger.LogInformation("Mapped school level from '{Original}' to ID: 3 (PARAGAMES)", originalLevel);
            return 3;
        }

        // Handle DEMO SPORTS specifically - map to ID 2 (same as Secondary)
        if (level.Contains("DEMO SPORTS"))
        {
            _logger.LogInformation("Mapped school level from '{Original}' to ID: 2 (Secondary/Demo Sports)",
                originalLevel);
            return 2;
        }

        // Direct ID mapping based on your exact requirements
        if (level.Contains("ELEMENTARY"))
        {
            _logger.LogInformation("Mapped school level from '{Original}' to ID: 1 (Elementary)", originalLevel);
            return 1;
        }
        else if (level.Contains("SECONDARY"))
        {
            _logger.LogInformation("Mapped school level from '{Original}' to ID: 2 (Secondary)", originalLevel);
            return 2;
        }

        // Default mapping for unrecognized levels
        _logger.LogWarning("Unrecognized school level '{Original}', defaulting to ID: 2 (Secondary)", originalLevel);
        return 2;
    }

// NEW: Helper method to resolve SchoolRegionID
    private async Task<int?> ResolveSchoolRegionId(string regionName)
    {
        if (string.IsNullOrWhiteSpace(regionName))
        {
            _logger.LogWarning("School region is empty");
            return null;
        }

        var regions = await _db.SchoolRegions
            .AsNoTracking()
            .ToListAsync();

        var region = regions.FirstOrDefault(sr =>
            sr.Region != null &&
            sr.Region.Trim().Equals(regionName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (region == null)
        {
            _logger.LogWarning("School region not found in database: {RegionName}", regionName);
        }

        return region?.ID;
    }

    private async Task<int?> ResolveSchoolDivisionId(string divisionName)
    {
        if (string.IsNullOrWhiteSpace(divisionName))
        {
            _logger.LogWarning("School division is empty");
            return null;
        }

        try
        {
            var divisions = await _db.SchoolDivisions.AsNoTracking().ToListAsync();
            var division = divisions.FirstOrDefault(sd =>
                sd.Division != null &&
                sd.Division.Trim().Equals(divisionName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (division == null)
            {
                _logger.LogWarning("School division not found in database: {DivisionName}", divisionName);
                return null;
            }

            _logger.LogInformation("Resolved division '{DivisionName}' to ID: {DivisionId}", divisionName, division.ID);
            return division.ID;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving school division: {DivisionName}", divisionName);
            return null;
        }
    }

    private async Task PersistBatchAsync(List<ProfileCoaches> batch)
    {
        if (!batch.Any()) return;

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.ProfileCoaches.AddRange(batch);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            _logger.LogInformation("Successfully inserted {Count} coaches", batch.Count);
        }
        catch (DbUpdateException dbEx)
        {
            await tx.RollbackAsync();
            _logger.LogError(dbEx, "Database error inserting batch of {Count} coaches", batch.Count);

            if (dbEx.InnerException?.Message.Contains("duplicate key") == true)
            {
                _logger.LogError("Duplicate coach ID detected in batch");
            }

            throw;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Error inserting batch of {Count} coaches", batch.Count);
            throw;
        }
    }
}