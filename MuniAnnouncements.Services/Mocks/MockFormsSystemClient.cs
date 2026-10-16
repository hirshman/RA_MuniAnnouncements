using MuniAnnouncements.Core.Interfaces;
using MuniAnnouncements.Core.Models.External;

namespace MuniAnnouncements.Services.Mocks;

/// <summary>
/// מימוש דמה של מערכת הטפסים – מחזיר נתוני Hard-Coded לצורך פיתוח ובדיקות
/// </summary>
public class MockFormsSystemClient : IFormsSystemClient
{
    public Task<IEnumerable<RawRequest>> GetRequestsByApplicantAsync(string idNumber)
    {
        var requests = new List<RawRequest>
        {
            new()
            {
                RequestId   = "F-88213",
                FormCode    = "ARNONA_DISCOUNT",
                AuthorityId = "6500",
                ApplicantId = idNumber,
                StatusCode  = "1",
                SubmittedAt = new DateTime(2026, 9, 2, 10, 15, 0, DateTimeKind.Utc),
                LastUpdate  = new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                RequestId   = "F-88214",
                FormCode    = "PARKING_PERMIT",
                AuthorityId = "6500",
                ApplicantId = idNumber,
                StatusCode  = "2",
                SubmittedAt = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc),
                LastUpdate  = new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                // formCode לא קיים בקטלוג – אמור להיות מסונן
                RequestId   = "F-88215",
                FormCode    = "UNKNOWN_FORM",
                AuthorityId = "6500",
                ApplicantId = idNumber,
                StatusCode  = "1",
                SubmittedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                LastUpdate  = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        return Task.FromResult<IEnumerable<RawRequest>>(requests);
    }
}
