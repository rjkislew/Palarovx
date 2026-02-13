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

        int headerRow = DetectHeaderRow(worksheet);
        if (headerRow == -1)
        {
            result.Errors.Add("Header row not detected.");
            return result;
        }

        var headerMap = BuildHeaderMap(worksheet, headerRow);

        var colMapping = GetColumnMappings(headerMap);
        if (colMapping.LastName == -1 || colMapping.FirstName == -1)
        {
            result.Errors.Add("Could not find 'Last Name' or 'First Name' columns.");
            return result;
        }

        // Sports cache (normalized sport + category)
        var sportsCache = await _db.Sports
            .AsNoTracking()
            .Where(s => !string.IsNullOrEmpty(s.Sport))
            .ToDictionaryAsync(
                s => $"{NormalizeSportName(s.Sport.Trim())}|{GetCategoryName(s.SportCategoryID)}",
                s => s.ID,
                StringComparer.OrdinalIgnoreCase
            );

        // Regions cache: accept both Region + Abbreviation
        var regionRows = await _db.SchoolRegions.AsNoTracking().ToListAsync();
        var regionCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var rr in regionRows)
        {
            if (!string.IsNullOrWhiteSpace(rr.Region))
                regionCache[rr.Region.Trim()] = rr.ID;

            if (!string.IsNullOrWhiteSpace(rr.Abbreviation))
                regionCache[rr.Abbreviation.Trim()] = rr.ID;
        }

        // Divisions cache: allow duplicates by grouping
        var divisionRows = await _db.SchoolDivisions
            .AsNoTracking()
            .Where(d => d.Division != null && d.Division.Trim() != "")
            .ToListAsync();

        var divisionCache = divisionRows
            .GroupBy(d => d.Division!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ID).Distinct().ToList(),
                StringComparer.OrdinalIgnoreCase
            );

        // Schools cache: Name + Level + Region
        var schoolsByNameList = await _db.Schools.AsNoTracking().ToListAsync();
        var schoolCacheByNameLevelRegion = schoolsByNameList
            .GroupBy(s => $"{(s.School ?? "").Trim()}|{(s.SchoolLevelsID ?? 0)}|{(s.SchoolRegionID ?? 0)}",
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var coachesBatch = new List<ProfileCoaches>();

        // Deduplicate within the uploaded file itself
        var uploadFingerprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

                var coach = await ProcessCoachRow(
                    worksheet,
                    r,
                    colMapping,
                    sportsCache,
                    regionCache,
                    divisionCache,
                    schoolCacheByNameLevelRegion,
                    uploadedBy
                );

                if (coach == null)
                {
                    result.SkippedCount++;
                    continue;
                }

                // Upload-file dedupe
                var fp = CreateCoachFingerprint(coach);
                if (!uploadFingerprints.Add(fp))
                {
                    result.SkippedCount++;
                    continue;
                }

                coachesBatch.Add(coach);

                if (coachesBatch.Count >= BatchSize)
                {
                    var inserted = await PersistBatchAsync(coachesBatch);
                    coachesBatch.Clear();
                    result.ImportedCount += inserted;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing row {Row}", r);
                result.Errors.Add($"Row {r}: {ex.Message}");
                result.SkippedCount++;
            }
        }

        if (coachesBatch.Any())
        {
            var inserted = await PersistBatchAsync(coachesBatch);
            result.ImportedCount += inserted;
        }

        return result;
    }

    // -----------------------------
    // Row -> entity
    // -----------------------------
    private async Task<ProfileCoaches?> ProcessCoachRow(
        ExcelWorksheet worksheet,
        int row,
        (int LastName, int FirstName, int MI, int Sex, int BirthDate, int Designation, int Sport, int GenderCategory,
            int SchoolName, int SchoolLevel, int SchoolDivision, int Region) cols,
        Dictionary<string, int> sportsCache,
        Dictionary<string, int> regionCache,
        Dictionary<string, List<int>> divisionCache,
        Dictionary<string, Schools> schoolCacheByNameLevelRegion,
        string uploadedBy)
    {
        string lastName = GetCellValue(worksheet, row, cols.LastName);
        string firstName = GetCellValue(worksheet, row, cols.FirstName);

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

        // Override rule you had
        if (!string.IsNullOrWhiteSpace(sportName))
        {
            string normalizedSport = sportName.Trim().ToUpperInvariant();
            if (normalizedSport.Contains("PENCAK SILAT") || normalizedSport.Contains("WEIGHTLIFTING"))
            {
                level = "SECONDARY";
                category = "Demo Sports";
                sportCategoryId = 2;
            }
        }

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

        int? sportId = null;
        if (cols.Sport != -1 && !string.IsNullOrWhiteSpace(sportName))
        {
            var normalizedSportName = NormalizeSportName(sportName);
            var searchKeys = new List<string> { $"{normalizedSportName}|{category.Trim()}" };

            if (category.Equals("Paragames", StringComparison.OrdinalIgnoreCase))
                searchKeys.Add($"{normalizedSportName}|Regular Sports");

            foreach (var k in searchKeys)
            {
                if (sportsCache.TryGetValue(k, out var sid))
                {
                    sportId = sid;
                    break;
                }
            }
        }

        int? genderCategoryId = ResolveGenderCategoryId(worksheet, row, cols.GenderCategory);

        // RegionID from cache
        string regionText = GetCellValue(worksheet, row, cols.Region);
        int? regionId = null;
        if (!string.IsNullOrWhiteSpace(regionText) &&
            regionCache.TryGetValue(regionText.Trim(), out var rid) && rid > 0)
        {
            regionId = rid;
        }

        // DivisionID from cache (duplicates allowed)
        string divisionText = GetCellValue(worksheet, row, cols.SchoolDivision);
        int? divisionId = null;
        if (!string.IsNullOrWhiteSpace(divisionText) &&
            divisionCache.TryGetValue(divisionText.Trim(), out var divIds) &&
            divIds != null && divIds.Count > 0)
        {
            divisionId = divIds[0];
        }

        int? schoolId = await ResolveSchoolId(
            worksheet,
            row,
            (cols.SchoolName, cols.SchoolLevel, cols.SchoolDivision, cols.Region),
            schoolCacheByNameLevelRegion,
            regionId,
            divisionId,
            level
        );

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
            SchoolRegionID = regionId,
            SchoolDivisionID = divisionId,
            SportCategoryID = sportCategoryId,
            UploadedBy = uploadedBy
        };
    }

    private async Task<int?> ResolveSchoolId(
        ExcelWorksheet worksheet,
        int row,
        (int SchoolName, int SchoolLevel, int SchoolDivision, int Region) cols,
        Dictionary<string, Schools> schoolCacheByNameLevelRegion,
        int? regionId,
        int? schoolDivisionId,
        string? overriddenLevel = null)
    {
        string schoolName = GetCellValue(worksheet, row, cols.SchoolName);
        string schoolLevel = overriddenLevel ?? GetCellValue(worksheet, row, cols.SchoolLevel);

        schoolName = (schoolName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(schoolName))
            return null;

        int? mappedSchoolLevelId = MapSchoolLevelToId(schoolLevel);

        string key = $"{schoolName}|{(mappedSchoolLevelId ?? 0)}|{(regionId ?? 0)}";

        if (schoolCacheByNameLevelRegion.TryGetValue(key, out var existingSchool))
            return existingSchool.ID;

        var newSchool = new Schools
        {
            School = schoolName,
            SchoolLevelsID = mappedSchoolLevelId,
            SchoolDivisionID = schoolDivisionId,
            SchoolRegionID = regionId,
            SchoolType = "Public"
        };

        _db.Schools.Add(newSchool);
        await _db.SaveChangesAsync();

        schoolCacheByNameLevelRegion.TryAdd(key, newSchool);
        return newSchool.ID;
    }

    // -----------------------------
    // Deduping
    // -----------------------------
    private static string Norm(string? s) => (s ?? "").Trim().ToUpperInvariant();

    private static string CreateCoachFingerprint(ProfileCoaches c)
    {
        // Ignore ID + UploadedBy, match the row’s actual data
        return string.Join("|",
            Norm(c.LastName),
            Norm(c.FirstName),
            Norm(c.MiddleInitial),
            Norm(c.Sex),
            c.BirthDate?.Date.ToString("yyyy-MM-dd") ?? "",
            Norm(c.Designation),
            (c.SportID?.ToString() ?? ""),
            (c.GenderCategoryID?.ToString() ?? ""),
            (c.SportCategoryID?.ToString() ?? ""),
            (c.SchoolID?.ToString() ?? ""),
            (c.SchoolRegionID?.ToString() ?? ""),
            (c.SchoolDivisionID?.ToString() ?? "")
        );
    }

    // Returns number of inserted rows
    private async Task<int> PersistBatchAsync(List<ProfileCoaches> batch)
    {
        if (!batch.Any()) return 0;

        var distinctBatch = batch
            .GroupBy(CreateCoachFingerprint, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        // Candidate fetch (broad), exact compare in memory
        var firstNames = distinctBatch.Select(b => b.FirstName?.Trim()).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var lastNames = distinctBatch.Select(b => b.LastName?.Trim()).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var candidates = await _db.ProfileCoaches
            .AsNoTracking()
            .Where(c =>
                c.FirstName != null && c.LastName != null &&
                firstNames.Contains(c.FirstName) &&
                lastNames.Contains(c.LastName))
            .ToListAsync();

        var existingFp = new HashSet<string>(candidates.Select(CreateCoachFingerprint), StringComparer.OrdinalIgnoreCase);

        var toInsert = distinctBatch
            .Where(c => !existingFp.Contains(CreateCoachFingerprint(c)))
            .ToList();

        if (!toInsert.Any())
            return 0;

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.ProfileCoaches.AddRange(toInsert);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            _logger.LogInformation("Inserted {Count} new coaches (duplicates skipped)", toInsert.Count);
            return toInsert.Count;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // -----------------------------
    // Helpers (sport / header / mapping)
    // -----------------------------
    private int? ResolveGenderCategoryId(ExcelWorksheet worksheet, int row, int genderCategoryCol)
    {
        if (genderCategoryCol == -1) return null;

        var genderCategory = GetCellValue(worksheet, row, genderCategoryCol);
        if (string.IsNullOrWhiteSpace(genderCategory)) return null;

        return genderCategory.Trim().ToUpperInvariant() switch
        {
            "B" or "BOYS" or "M" or "MALE" => BOYS_GENDER_ID,
            "G" or "GIRLS" or "F" or "FEMALE" => GIRLS_GENDER_ID,
            "B/G" or "MIXED" or "COED" or "BOTH" => MIXED_GENDER_ID,
            _ => null
        };
    }

    private string GenerateCoachId(string firstName, string lastName, int? sportId)
    {
        if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName) && sportId.HasValue)
        {
            var namePart = $"{firstName.Substring(0, Math.Min(3, firstName.Length))}{lastName.Substring(0, Math.Min(3, lastName.Length))}";
            return $"C{namePart.ToUpperInvariant()}{sportId.Value}{DateTime.Now:HHmmss}";
        }

        return $"C{Guid.NewGuid().ToString("N")[..19]}";
    }

    private string NormalizeSportName(string sportName)
    {
        if (string.IsNullOrWhiteSpace(sportName))
            return sportName;

        string normalized = sportName.Trim().ToUpperInvariant();

        if (normalized.Contains("BASKETBALL")) return "BASKETBALL";
        if (normalized.Contains("DANCE SPORTS") || normalized.Contains("DANCESPORT") || normalized.Contains("DANCE")) return "DANCE SPORTS";
        if (normalized.Contains("GYMNASTICS")) return "GYMNASTICS";
        if (normalized.Contains("VOLLEYBALL")) return "VOLLEYBALL";
        if (normalized.Contains("FOOTBALL")) return "FOOTBALL";
        if (normalized.Contains("SWIMMING")) return "SWIMMING";
        if (normalized.Contains("ATHLETICS") || normalized.Contains("TRACK AND FIELD")) return "ATHLETICS";
        if (normalized.Contains("TABLE TENNIS")) return "TABLE TENNIS";
        if (normalized.Contains("BADMINTON")) return "BADMINTON";
        if (normalized.Contains("CHESS")) return "CHESS";
        if (normalized.Contains("FUTZAL")) return "FUTSAL";

        var index = normalized.IndexOf('(');
        if (index > 0)
            normalized = normalized.Substring(0, index).Trim();

        return normalized;
    }

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
                rowValues.Add((worksheet.Cells[r, c].Text ?? "").Trim().ToLowerInvariant());

            if (rowValues.Any(v => v.Contains("lastname") || v.Contains("last name")) &&
                rowValues.Any(v => v.Contains("firstname") || v.Contains("first name")) &&
                rowValues.Any(v => v.Contains("designation")) &&
                (rowValues.Any(v => v.Contains("event")) || rowValues.Any(v => v.Contains("sport"))))
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
                headerMap[headerText] = c;
        }

        return headerMap;
    }

    private (int LastName, int FirstName, int MI, int Sex, int BirthDate, int Designation, int Sport, int GenderCategory,
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
                    if (key.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0)
                        return headerMap[key];
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
            GetCol("event", "sport", "sports"),
            GetCol("b_or_g", "gender category", "category", "gender"),
            GetCol("schoolname", "school", "schoolname"),
            GetCol("level", "school level", "schoollevel"),
            GetCol("school division", "division", "schooldivision"),
            GetCol("region")
        );
    }

    private bool RowHasData(ExcelWorksheet worksheet, int row, int lastCol)
    {
        for (int c = 1; c <= lastCol; c++)
            if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, c].Text))
                return true;
        return false;
    }

    private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        => col != -1 ? (worksheet.Cells[row, col].Text ?? "").Trim() : "";

    private int? MapSchoolLevelToId(string originalLevel)
    {
        if (string.IsNullOrWhiteSpace(originalLevel))
            return 2;

        string level = originalLevel.Trim().ToUpperInvariant();

        if (level.Contains("PARAGAMES")) return 3;
        if (level.Contains("DEMO SPORTS")) return 2;
        if (level.Contains("ELEMENTARY")) return 1;
        if (level.Contains("SECONDARY")) return 2;

        return 2;
    }
}
