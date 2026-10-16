namespace MuniAnnouncements.Core.Models.External;

/// <summary>
/// בקשה גולמית כפי שמתקבלת ממערכת הטפסים
/// </summary>
public class RawRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string FormCode { get; set; } = string.Empty;
    public string AuthorityId { get; set; } = string.Empty;
    // applicantId נשמר פנימית בלבד – אסור להחזיר ללקוח
    public string ApplicantId { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime LastUpdate { get; set; }
}
