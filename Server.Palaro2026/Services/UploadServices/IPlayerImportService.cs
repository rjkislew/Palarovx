using Server.Palaro2026.DTO;

namespace Server.Palaro2026.Services.UploadServices;

public interface IPlayerImportService
{
    Task<ImportResult> ImportPlayersFromExcelAsync(Stream excelStream, string uploadedBy = "system");
}
