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

        // Header detection
        int headerRow = DetectHeaderRow(worksheet);
        if (headerRow == -1)
        {
            result.Errors.Add("Header row not detected.");
            return result;
        }

        var headerMap = BuildHeaderMap(worksheet, headerRow);

        // Get column indexes
        var colMapping = GetColumnMappings(headerMap);
        if (colMapping.LastName == -1 || colMapping.FirstName == -1)
        {
            result.Errors.Add("Could not find 'Last Name' or 'First Name' columns.");
            return result;
        }

        // UPDATED: Pre-load lookup data with normalized sport names
        var sportsCache = await _db.Sports
            .AsNoTracking()
            .Where(s => !string.IsNullOrEmpty(s.Sport))
            .ToDictionaryAsync(
                s => $"{NormalizeSportName(s.Sport.Trim())}|{GetCategoryName(s.SportCategoryID)}", // Use normalized names
                s => s.ID,
                StringComparer.OrdinalIgnoreCase
            );

        // NEW: Pre-load regions/divisions for fast lookups (prevents per-row DB hits)
        var regionRows = await _db.SchoolRegions.AsNoTracking().ToListAsync();

        var regionCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var r in regionRows)
        {
            if (!string.IsNullOrWhiteSpace(r.Region))
                regionCache[r.Region.Trim()] = r.ID;

            if (!string.IsNullOrWhiteSpace(r.Abbreviation))
                regionCache[r.Abbreviation.Trim()] = r.ID;
        }


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
        // Schools cache by name (Name + Level + Region)
        var schoolsByNameList = await _db.Schools.AsNoTracking().ToListAsync();

        var schoolCacheByNameAndLevel = schoolsByNameList
            .GroupBy(s => $"{(s.School ?? "").Trim()}|{(s.SchoolLevelsID ?? 0)}|{(s.SchoolRegionID ?? 0)}",
                     StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);


        var playersBatch = new List<ProfilePlayers>();
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

                if (string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(firstName) &&
                    string.IsNullOrWhiteSpace(lrn))
                {
                    result.SkippedCount++;
                    continue;
                }

                // Process the row
                var player = await ProcessPlayerRow(
    worksheet,
    r,
    colMapping,
    sportsCache,
    schoolCacheByCode,
    schoolCacheByNameAndLevel,
    regionCache,
    divisionCache,
    uploadedBy
);
                if (player != null)
                {
                    playersBatch.Add(player);
                }

                if (playersBatch.Count >= BatchSize)
                {
                    await PersistBatchAsync(playersBatch);
                    playersBatch.Clear();
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
            await PersistBatchAsync(playersBatch);
        }

        result.ImportedCount = result.TotalRows - result.SkippedCount - result.Errors.Count;
        return result;
    }

    // NEW: Method to normalize sport names from Excel
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
        else if (normalized.Contains("DANCE SPORTS") || normalized.Contains("DANCESPORT") || normalized.Contains("DANCE"))
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
        else if (normalized.Contains("SEPAK TAKRAW"))
        {
            return "SEPAK TAKRAW"; // Always map to CHESS
        }
        else if (normalized.Contains("BOCCE"))
        {
            return "BOCCE"; // Always map to CHESS
        }

        // For other sports, return as is but remove any parenthetical content
        var index = normalized.IndexOf('(');
        if (index > 0)
        {
            normalized = normalized.Substring(0, index).Trim();
        }

        return normalized;
    }

    private string GetCategoryName(int? sportCategoryId)
    {
        // Map SportCategoryID to category names
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

            // UPDATED: Match the actual Excel headers
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
            {
                headerMap[headerText] = c;
            }
        }

        return headerMap;
    }

    private (int LastName, int FirstName, int LRN, int Sport, int SchoolName, int SchoolCode, int MI, int Sex, int Bdate
        , int Category, int SchoolLevel, int SchoolDivision, int SchoolRegion, int SchoolType, int SchoolAddress)
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
            GetCol("school type", "schooltype", "type"), // NEW
            GetCol("school address", "address", "schooladdress") // NEW
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
        return col != -1 ? worksheet.Cells[row, col].Text.Trim() : "";
    }

    private async Task<ProfilePlayers?> ProcessPlayerRow(
    ExcelWorksheet worksheet,
    int row,
    (int LastName, int FirstName, int LRN, int Sport, int SchoolName, int SchoolCode, int MI, int Sex, int Bdate,
     int Category, int SchoolLevel, int SchoolDivision, int SchoolRegion, int SchoolType, int SchoolAddress) cols,
    Dictionary<string, int> sportsCache,
    Dictionary<string, Schools> schoolCacheByCode,
    Dictionary<string, Schools> schoolCacheByNameAndLevel,
    Dictionary<string, int> regionCache,
   Dictionary<string, List<int>> divisionCache,
    string uploadedBy)
    {
        string lastName = GetCellValue(worksheet, row, cols.LastName);
        string firstName = GetCellValue(worksheet, row, cols.FirstName);
        string lrn = GetCellValue(worksheet, row, cols.LRN);

        // Parse birthdate
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
        string category = "Regular Sports"; // Default
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

        // UPDATED: Resolve SportID with normalized names and category-based search
        int? sportId = null;
        if (cols.Sport != -1)
        {
            var sportName = GetCellValue(worksheet, row, cols.Sport);

            if (!string.IsNullOrWhiteSpace(sportName))
            {
                // Normalize the sport name from Excel
                var normalizedSportName = NormalizeSportName(sportName);

                // Create search keys for different category scenarios
                var searchKeys = new List<string>();

                // Always try the exact category first
                var exactKey = $"{normalizedSportName}|{category.Trim()}";
                searchKeys.Add(exactKey);

                // If it's PARAGAMES, also try searching in Regular category as fallback
                if (category.Equals("Paragames", StringComparison.OrdinalIgnoreCase))
                {
                    searchKeys.Add($"{normalizedSportName}|Regular Sports");
                }

                // Try each search key until we find a match
                foreach (var searchKey in searchKeys)
                {
                    if (sportsCache.TryGetValue(searchKey, out var sid))
                    {
                        sportId = sid;
                        _logger.LogInformation("Found sport: {NormalizedSport} (Original: {OriginalSport}) with Category: {Category}",
                            normalizedSportName, sportName, category);
                        break;
                    }
                }

                if (!sportId.HasValue)
                {
                    _logger.LogWarning("Sport not found: {OriginalSport} -> {NormalizedSport} (Category: {Category})",
                        sportName, normalizedSportName, category);
                }
            }
        }

        // Resolve SchoolID
        int? schoolId = await ResolveSchoolId(
        worksheet, row,
        (cols.SchoolName, cols.SchoolCode, cols.SchoolLevel, cols.SchoolDivision, cols.SchoolRegion, cols.SchoolType, cols.SchoolAddress),
        schoolCacheByCode, schoolCacheByNameAndLevel,
        regionCache, divisionCache
    );

        // Generate a unique ID for the player
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

    // Helper method to generate player IDs
    private string GeneratePlayerId(string firstName, string lastName, string lrn, int? sportId)
    {
        // Option 1: Use LRN + SportID if available
        if (!string.IsNullOrWhiteSpace(lrn) && sportId.HasValue)
        {
            return $"{lrn.Trim()}-{sportId.Value}";
        }

        // Option 2: Use name-based ID with timestamp
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var namePart =
            $"{firstName?.Substring(0, Math.Min(3, firstName.Length)) ?? "FNU"}{lastName?.Substring(0, Math.Min(3, lastName.Length)) ?? "LNU"}";

        return $"{namePart.ToUpper()}{timestamp}";
    }

    // Alternative: Use GUID-based IDs
    private string GeneratePlayerId()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 20); // Match your 20 char limit
    }

    // NEW: Helper method to resolve category name to SportCategoryID
    private async Task<int?> ResolveSportCategoryId(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName)) return null;

        // You'll need to query your SportCategories table
        // This is a placeholder - adjust based on your actual database structure
        var category = await _db.SportCategories
            .FirstOrDefaultAsync(sc =>
                sc.Category.Trim().Equals(categoryName.Trim(), StringComparison.OrdinalIgnoreCase));

        return category?.ID;
    }

    private async Task<int?> ResolveSchoolId(
    ExcelWorksheet worksheet,
    int row,
    (int SchoolName, int SchoolCode, int SchoolLevel, int SchoolDivision, int SchoolRegion, int SchoolType, int SchoolAddress) cols,
    Dictionary<string, Schools> schoolCacheByCode,
    Dictionary<string, Schools> schoolCacheByNameAndLevel,
    Dictionary<string, int> regionCache,
    Dictionary<string, List<int>> divisionCache)
    {
        string schoolCode = GetCellValue(worksheet, row, cols.SchoolCode);
        string schoolName = GetCellValue(worksheet, row, cols.SchoolName);

        // 🔒 NORMALIZE early (this is the key fix)
        schoolCode = (schoolCode ?? "").Trim();
        schoolName = (schoolName ?? "").Trim();

        string schoolLevel = GetCellValue(worksheet, row, cols.SchoolLevel);
        string schoolDivision = GetCellValue(worksheet, row, cols.SchoolDivision);
        string schoolRegion = GetCellValue(worksheet, row, cols.SchoolRegion);

        string schoolType = GetCellValue(worksheet, row, cols.SchoolType);
        string schoolAddress = GetCellValue(worksheet, row, cols.SchoolAddress);

        int? mappedSchoolLevelId = MapSchoolLevelToId(schoolLevel);

        // Resolve IDs from caches
        int? schoolRegionId = null;
        if (!string.IsNullOrWhiteSpace(schoolRegion) && regionCache.TryGetValue(schoolRegion.Trim(), out var rid) && rid > 0)
            schoolRegionId = rid;

        int? schoolDivisionId = null;

        if (!string.IsNullOrWhiteSpace(schoolDivision) &&
            divisionCache.TryGetValue(schoolDivision.Trim(), out var divIds) &&
            divIds != null && divIds.Count > 0)
        {
            // If duplicates exist (e.g., San Fernando City appears twice), just pick one deterministically.
            // NOTE: If your SchoolDivisions table has a RegionID, we should select the one matching schoolRegionId instead.
            schoolDivisionId = divIds[0];

            if (divIds.Count > 1)
                _logger.LogWarning("Multiple division IDs found for '{Division}'. Using ID {ID}.", schoolDivision, schoolDivisionId);
        }

        // Composite keys (Level + Region)
        string codeCacheKey = $"{schoolCode}|{(mappedSchoolLevelId ?? 0)}|{(schoolRegionId ?? 0)}";
        string nameCacheKey = $"{schoolName}|{(mappedSchoolLevelId ?? 0)}|{(schoolRegionId ?? 0)}";

        // Match by Code+Level+Region
        if (!string.IsNullOrWhiteSpace(schoolCode) && schoolCacheByCode.TryGetValue(codeCacheKey, out var byCode))
            return byCode.ID;

        // Match by Name+Level+Region
        if (!string.IsNullOrWhiteSpace(schoolName) && schoolCacheByNameAndLevel.TryGetValue(nameCacheKey, out var byName))
            return byName.ID;

        // Create new if not found (allowed when Region or Level differs)
        if (!string.IsNullOrWhiteSpace(schoolName))
        {
            var newSchool = new Schools
            {
                School = schoolName,
                SchoolCode = schoolCode,
                SchoolLevelsID = mappedSchoolLevelId,
                SchoolDivisionID = schoolDivisionId,
                SchoolRegionID = schoolRegionId,
                SchoolType = schoolType,
                SchoolAddress = schoolAddress
            };

            _db.Schools.Add(newSchool);
            await _db.SaveChangesAsync();

            // Update caches with the same composite rule
            if (!string.IsNullOrEmpty(newSchool.SchoolCode) && mappedSchoolLevelId.HasValue)
            {
                var k = $"{newSchool.SchoolCode}|{mappedSchoolLevelId.Value}|{(schoolRegionId ?? 0)}";
                if (!schoolCacheByCode.ContainsKey(k)) schoolCacheByCode[k] = newSchool;
            }

            var nk = $"{newSchool.School}|{(mappedSchoolLevelId ?? 0)}|{(schoolRegionId ?? 0)}";
            if (!schoolCacheByNameAndLevel.ContainsKey(nk)) schoolCacheByNameAndLevel[nk] = newSchool;

            return newSchool.ID;
        }

        return null;
    }

    // UPDATED: School level mapping method with exact requirements
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
            _logger.LogInformation("Mapped school level from '{Original}' to ID: 2 (Secondary/Demo Sports)", originalLevel);
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

    private async Task<int?> ResolveSchoolDivisionId(string divisionName)
    {
        if (string.IsNullOrWhiteSpace(divisionName))
        {
            _logger.LogWarning("School division is empty");
            return null;
        }

        // FIXED: Use EF-compatible query
        var divisions = await _db.SchoolDivisions
            .AsNoTracking()
            .ToListAsync(); // Bring to client first

        var division = divisions.FirstOrDefault(sd =>
            sd.Division != null &&
            sd.Division.Trim().Equals(divisionName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (division == null)
        {
            _logger.LogWarning("School division not found in database: {DivisionName}", divisionName);
        }

        return division?.ID;
    }

    private async Task<int?> ResolveSchoolRegionId(string regionName)
    {
        if (string.IsNullOrWhiteSpace(regionName))
        {
            _logger.LogWarning("School region is empty");
            return null;
        }

        // FIXED: Use EF-compatible query
        var regions = await _db.SchoolRegions
            .AsNoTracking()
            .ToListAsync(); // Bring to client first

        var region = regions.FirstOrDefault(sr =>
            sr.Region != null &&
            sr.Region.Trim().Equals(regionName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (region == null)
        {
            _logger.LogWarning("School region not found in database: {RegionName}", regionName);
        }

        return region?.ID;
    }

    private async Task PersistBatchAsync(List<ProfilePlayers> batch)
    {
        if (!batch.Any()) return;

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // CHECK FOR DUPLICATE IDs BEFORE INSERTING
            var duplicateIds = batch
                .GroupBy(p => p.ID)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIds.Any())
            {
                _logger.LogWarning("Found duplicate player IDs in batch: {DuplicateIds}",
                    string.Join(", ", duplicateIds));

                // Regenerate IDs for duplicates
                foreach (var player in batch.Where(p => duplicateIds.Contains(p.ID)))
                {
                    player.ID = GeneratePlayerId(); // Use GUID fallback
                }
            }

            var existingIds = await _db.ProfilePlayers
                .Where(p => batch.Select(b => b.ID).Contains(p.ID))
                .Select(p => p.ID)
                .ToListAsync();

            if (existingIds.Any())
            {
                _logger.LogWarning("Players already exist in database with IDs: {ExistingIds}",
                    string.Join(", ", existingIds));

                // Remove duplicates or regenerate IDs
                var playersToAdd = batch.Where(p => !existingIds.Contains(p.ID)).ToList();
                var playersToUpdate = batch.Where(p => existingIds.Contains(p.ID)).ToList();

                _logger.LogInformation("Adding {NewCount} new players, skipping {ExistingCount} existing players",
                    playersToAdd.Count, playersToUpdate.Count);

                _db.ProfilePlayers.AddRange(playersToAdd);
            }
            else
            {
                _db.ProfilePlayers.AddRange(batch);
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            _logger.LogInformation("Successfully inserted {Count} players", batch.Count);
        }
        catch (DbUpdateException dbEx)
        {
            await tx.RollbackAsync();
            _logger.LogError(dbEx, "Database error inserting batch of {Count} players", batch.Count);

            // More detailed error handling
            if (dbEx.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerException}", dbEx.InnerException.Message);

                if (dbEx.InnerException.Message.Contains("duplicate key"))
                {
                    // Log the problematic IDs for debugging
                    var problematicIds = batch.Select(p => p.ID).ToList();
                    _logger.LogError("Duplicate key violation with IDs: {ProblematicIds}",
                        string.Join(", ", problematicIds));
                }
            }

            throw;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Error inserting batch of {Count} players", batch.Count);
            throw;
        }
    }
}