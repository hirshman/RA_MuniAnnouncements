using MuniAnnouncements.Core.Models.External;

namespace MuniAnnouncements.Core.Interfaces;

/// <summary>
/// ממשק לפנייה למערכת הטפסים החיצונית
/// </summary>
public interface IFormsSystemClient
{
    Task<IEnumerable<RawRequest>> GetRequestsByApplicantAsync(string idNumber);
}
