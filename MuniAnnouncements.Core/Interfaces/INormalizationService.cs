using MuniAnnouncements.Core.Models.Response;

namespace MuniAnnouncements.Core.Interfaces;

/// <summary>
/// ממשק לנרמול בקשות גולמיות לאובייקט תשובה ללקוח
/// </summary>
public interface INormalizationService
{
    Task<List<NormalizedRequest>> NormalizeAsync(string idNumber);
}
