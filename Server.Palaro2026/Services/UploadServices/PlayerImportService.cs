using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Services.UploadServices;

public class PlayerImportService : IPlayerImportService
{
    private readonly Palaro2026Context _db;
    private readonly ILogger<PlayerImportService> _logger;
    private const int BatchSize = 200;

    public PlayerImportService(Palaro2026Context db, ILogger<PlayerImportService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ImportResult> ImportPlayersFromExcelAsync(Stream excelStream, string uploadedBy = "system")
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

        // Schools cache by code (Code + Level + Region)
        var schoolsByCodeList = await _db.Schools.AsNoTracking()
            .Where(s => !string.IsNullOrEmpty(s.SchoolCode) && s.SchoolLevelsID.HasValue)
            .ToListAsync();

        var schoolCacheByCode = schoolsByCodeList
            .GroupBy(s => $"{(s.SchoolCode ?? "").Trim()}|{s.SchoolLevelsID.Value}|{(s.SchoolRegionID ?? 0)}",
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Schools cache by name (Name + Level + Region)
        var schoolsByNameList = await _db.Schools.AsNoTracking().ToListAsync();
        var schoolCacheByNameLevelRegion = schoolsByNameList
            .GroupBy(s => $"{(s.School ?? "").Trim()}|{(s.SchoolLevelsID ?? 0)}|{(s.SchoolRegionID ?? 0)}",
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var playersBatch = new List<ProfilePlayers>();

        // Deduplicate within the uploaded file itself (same row repeated)
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
                string lrn = GetCellValue(worksheet, r, colMapping.LRN);

                if (string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lrn))
                {
                    result.SkippedCount++;
                    continue;
                }

                var player = await ProcessPlayerRow(
                    worksheet,
                    r,
                    colMapping,
                    sportsCache,
                    schoolCacheByCode,
                    schoolCacheByNameLevelRegion,
                    regionCache,
                    divisionCache,
                    uploadedBy
                );

                if (player == null)
                {
                    result.SkippedCount++;
                    continue;
                }

                // Upload-file dedupe (exact match within file)
                var fp = CreatePlayerFingerprint(player);
                if (!uploadFingerprints.Add(fp))
                {
                    result.SkippedCount++;
                    continue;
                }

                playersBatch.Add(player);

                if (playersBatch.Count >= BatchSize)
                {
                    var inserted = await PersistBatchAsync(playersBatch);
                    playersBatch.Clear();
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

        // Final batch
        if (playersBatch.Any())
        {
            var inserted = await PersistBatchAsync(playersBatch);
            result.ImportedCount += inserted;
        }

        // ImportedCount is now tracked by actual inserts; don’t re-derive it.
        return result;
    }

    // -----------------------------
    // Sport normalization
    // -----------------------------
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
        if (normalized.Contains("SEPAK TAKRAW")) return "SEPAK TAKRAW";
        if (normalized.Contains("BOCCE")) return "BOCCE";

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

    // -----------------------------
    // Header / mapping helpers
    // -----------------------------
    private int DetectHeaderRow(ExcelWorksheet worksheet)
    {
        var maxHeaderScan = Math.Min(40, worksheet.Dimension.End.Row);
        for (int r = 1; r <= maxHeaderScan; r++)
        {
            var rowValues = new List<string>();
            for (int c = 1; c <= worksheet.Dimension.End.Column; c++)
                rowValues.Add((worksheet.Cells[r, c].Text ?? "").Trim().ToLowerInvariant());

            if (rowValues.Any(v => v.Contains("last name")) &&
                rowValues.Any(v => v.Contains("first name")) &&
                rowValues.Any(v => v.Contains("lrn")) &&
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

    private (int LastName, int FirstName, int LRN, int Sport, int SchoolName, int SchoolCode, int MI, int Sex, int Bdate,
        int Category, int SchoolLevel, int SchoolDivision, int SchoolRegion, int SchoolType, int SchoolAddress)
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
            GetCol("last name", "lastname", "last"),
            GetCol("first name", "firstname", "first"),
            GetCol("lrn"),
            GetCol("event", "sport", "sport name", "sports"),
            GetCol("school name", "school", "schoolname"),
            GetCol("school code", "schoolcode", "school_code", "sch code", "schcode"),
            GetCol("mi", "middle initial", "middle"),
            GetCol("sex"),
            GetCol("bdate", "birthdate", "birth date", "birth"),
            GetCol("category", "sport category"),
            GetCol("level", "school level"),
            GetCol("schdiv", "school division", "division", "schooldivision"),
            GetCol("region", "school region"),
            GetCol("school type", "schooltype", "type"),
            GetCol("school address", "address", "schooladdress")
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

    // -----------------------------
    // Row -> entity
    // -----------------------------
    private async Task<ProfilePlayers?> ProcessPlayerRow(
        ExcelWorksheet worksheet,
        int row,
        (int LastName, int FirstName, int LRN, int Sport, int SchoolName, int SchoolCode, int MI, int Sex, int Bdate,
            int Category, int SchoolLevel, int SchoolDivision, int SchoolRegion, int SchoolType, int SchoolAddress) cols,
        Dictionary<string, int> sportsCache,
        Dictionary<string, Schools> schoolCacheByCode,
        Dictionary<string, Schools> schoolCacheByNameLevelRegion,
        Dictionary<string, int> regionCache,
        Dictionary<string, List<int>> divisionCache,
        string uploadedBy)
    {
        string lastName = GetCellValue(worksheet, row, cols.LastName);
        string firstName = GetCellValue(worksheet, row, cols.FirstName);
        string lrn = GetCellValue(worksheet, row, cols.LRN);

        DateTime? birthDate = null;
        if (cols.Bdate != -1)
        {
            var btext = GetCellValue(worksheet, row, cols.Bdate);
            if (!string.IsNullOrWhiteSpace(btext))
            {
                if (DateTime.TryParse(btext, out var dt))
                    birthDate = dt;
                else if (double.TryParse(btext, out var oa))
                    birthDate = DateTime.FromOADate(oa);
            }
        }

        string level = GetCellValue(worksheet, row, cols.SchoolLevel);
        string category = "Regular Sports";
        int? sportCategoryId = null;

        if (!string.IsNullOrWhiteSpace(level))
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
        if (cols.Sport != -1)
        {
            var sportName = GetCellValue(worksheet, row, cols.Sport);
            if (!string.IsNullOrWhiteSpace(sportName))
            {
                var normalizedSportName = NormalizeSportName(sportName);
                var searchKeys = new List<string>
                {
                    $"{normalizedSportName}|{category.Trim()}"
                };

                if (category.Equals("Paragames", StringComparison.OrdinalIgnoreCase))
                    searchKeys.Add($"{normalizedSportName}|Regular Sports");

                foreach (var key in searchKeys)
                {
                    if (sportsCache.TryGetValue(key, out var sid))
                    {
                        sportId = sid;
                        break;
                    }
                }
            }
        }

        int? schoolId = await ResolveSchoolId(
            worksheet,
            row,
            (cols.SchoolName, cols.SchoolCode, cols.SchoolLevel, cols.SchoolDivision, cols.SchoolRegion, cols.SchoolType, cols.SchoolAddress),
            schoolCacheByCode,
            schoolCacheByNameLevelRegion,
            regionCache,
            divisionCache
        );

        // ID generation is still needed for new inserts, but duplicates will be skipped before insert.
        string playerId = GeneratePlayerId(firstName, lastName, lrn, sportId);

        return new ProfilePlayers
        {
            ID = playerId,
            FirstName = firstName,
            LastName = lastName,
            MiddleInitial = GetCellValue(worksheet, row, cols.MI),
            Sex = GetCellValue(worksheet, row, cols.Sex),
            BirthDate = birthDate,
            LRN = lrn,
            SchoolID = schoolId,
            SportID = sportId,
            SportCategoryID = sportCategoryId,
            UploadedBy = uploadedBy
        };
    }

    private string GeneratePlayerId(string firstName, string lastName, string lrn, int? sportId)
    {
        if (!string.IsNullOrWhiteSpace(lrn) && sportId.HasValue)
            return $"{lrn.Trim()}-{sportId.Value}";

        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var namePart = $"{firstName?.Substring(0, Math.Min(3, firstName.Length)) ?? "FNU"}{lastName?.Substring(0, Math.Min(3, lastName.Length)) ?? "LNU"}";
        return $"{namePart.ToUpperInvariant()}{timestamp}";
    }

    // -----------------------------
    // School resolver (Name/Code + Level + Region)
    // -----------------------------
    private async Task<int?> ResolveSchoolId(
        ExcelWorksheet worksheet,
        int row,
        (int SchoolName, int SchoolCode, int SchoolLevel, int SchoolDivision, int SchoolRegion, int SchoolType, int SchoolAddress) cols,
        Dictionary<string, Schools> schoolCacheByCode,
        Dictionary<string, Schools> schoolCacheByNameLevelRegion,
        Dictionary<string, int> regionCache,
        Dictionary<string, List<int>> divisionCache)
    {
        string schoolCode = GetCellValue(worksheet, row, cols.SchoolCode);
        string schoolName = GetCellValue(worksheet, row, cols.SchoolName);

        schoolCode = (schoolCode ?? "").Trim();
        schoolName = (schoolName ?? "").Trim();

        string schoolLevel = GetCellValue(worksheet, row, cols.SchoolLevel);
        string schoolDivision = GetCellValue(worksheet, row, cols.SchoolDivision);
        string schoolRegion = GetCellValue(worksheet, row, cols.SchoolRegion);
        string schoolType = GetCellValue(worksheet, row, cols.SchoolType);
        string schoolAddress = GetCellValue(worksheet, row, cols.SchoolAddress);

        int? mappedSchoolLevelId = MapSchoolLevelToId(schoolLevel);

        // Resolve RegionID from cache (accept region OR abbreviation)
        int? schoolRegionId = null;
        if (!string.IsNullOrWhiteSpace(schoolRegion) &&
            regionCache.TryGetValue(schoolRegion.Trim(), out var rid) && rid > 0)
        {
            schoolRegionId = rid;
        }

        // Resolve DivisionID from cache (duplicates allowed; pick first deterministically)
        int? schoolDivisionId = null;
        if (!string.IsNullOrWhiteSpace(schoolDivision) &&
            divisionCache.TryGetValue(schoolDivision.Trim(), out var divIds) &&
            divIds != null && divIds.Count > 0)
        {
            schoolDivisionId = divIds[0];
        }

        string codeCacheKey = $"{schoolCode}|{(mappedSchoolLevelId ?? 0)}|{(schoolRegionId ?? 0)}";
        string nameCacheKey = $"{schoolName}|{(mappedSchoolLevelId ?? 0)}|{(schoolRegionId ?? 0)}";

        if (!string.IsNullOrWhiteSpace(schoolCode) && schoolCacheByCode.TryGetValue(codeCacheKey, out var byCode))
            return byCode.ID;

        if (!string.IsNullOrWhiteSpace(schoolName) && schoolCacheByNameLevelRegion.TryGetValue(nameCacheKey, out var byName))
            return byName.ID;

        if (string.IsNullOrWhiteSpace(schoolName))
            return null;

        var newSchool = new Schools
        {
            School = schoolName,
            SchoolCode = string.IsNullOrWhiteSpace(schoolCode) ? null : schoolCode,
            SchoolLevelsID = mappedSchoolLevelId,
            SchoolDivisionID = schoolDivisionId,
            SchoolRegionID = schoolRegionId,
            SchoolType = schoolType,
            SchoolAddress = schoolAddress
        };

        _db.Schools.Add(newSchool);
        await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(schoolCode) && mappedSchoolLevelId.HasValue)
            schoolCacheByCode.TryAdd(codeCacheKey, newSchool);

        schoolCacheByNameLevelRegion.TryAdd(nameCacheKey, newSchool);

        return newSchool.ID;
    }

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

    // -----------------------------
    // Exact-match dedupe helpers
    // -----------------------------
    private static string Norm(string? s) => (s ?? "").Trim().ToUpperInvariant();

    private static string CreatePlayerFingerprint(ProfilePlayers p)
    {
        // Exact row match (ignore ID + UploadedBy)
        // Include everything that represents the row’s identity
        return string.Join("|",
            Norm(p.LastName),
            Norm(p.FirstName),
            Norm(p.MiddleInitial),
            Norm(p.Sex),
            p.BirthDate?.Date.ToString("yyyy-MM-dd") ?? "",
            Norm(p.LRN),
            (p.SchoolID?.ToString() ?? ""),
            (p.SportID?.ToString() ?? ""),
            (p.SportCategoryID?.ToString() ?? "")
        );
    }

    // Returns number of inserted rows
    private async Task<int> PersistBatchAsync(List<ProfilePlayers> batch)
    {
        if (!batch.Any()) return 0;

        // De-duplicate inside batch
        var distinctBatch = batch
            .GroupBy(p => p.ID, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var ids = distinctBatch.Select(p => p.ID).ToList();

            var existingIds = await _db.ProfilePlayers
                .AsNoTracking()
                .Where(p => ids.Contains(p.ID))
                .Select(p => p.ID)
                .ToListAsync();

            var toInsert = distinctBatch
                .Where(p => !existingIds.Contains(p.ID))
                .ToList();

            if (toInsert.Any())
                _db.ProfilePlayers.AddRange(toInsert);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            _db.ChangeTracker.Clear();
            return toInsert.Count;   // ✅ THIS is why the method returns int
        }
        catch
        {
            await tx.RollbackAsync();
            _db.ChangeTracker.Clear();
            throw;
        }
    }
}
