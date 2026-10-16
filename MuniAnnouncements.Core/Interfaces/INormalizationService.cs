using MuniAnnouncements.Core.Models.Response;

namespace MuniAnnouncements.Core.Interfaces;

/// <summary>
/// ממשק לנרמול בקשות גולמיות לאובייקט תשובה ללקוח
/// </summary>
public interface INormalizationService
{
    Task<IEnumerable<NormalizedRequest>> NormalizeAsync(string idNumber);
}
