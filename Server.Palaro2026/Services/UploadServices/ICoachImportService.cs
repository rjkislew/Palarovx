using Server.Palaro2026.DTO;

namespace Server.Palaro2026.Services.UploadServices;

public interface ICoachImportService
{
    Task<ImportResult> ImportCoachesFromExcelAsync(Stream excelStream, string uploadedBy = "system");
}