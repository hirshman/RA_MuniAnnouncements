namespace MuniAnnouncements.Core.Models.Response;

/// <summary>
/// בקשה מנורמלת – האובייקט שמוחזר ללקוח (ללא ת"ז)
/// </summary>
public class NormalizedRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string AuthorityName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool ActionRequired { get; set; }
    public bool IsOverdue { get; set; }
    // פורמט DD/MM/YYYY
    public string SubmittedAt { get; set; } = string.Empty;
    public string LastUpdate { get; set; } = string.Empty;
    // שדה פנימי למיון – לא מוחזר ב-JSON
    [System.Text.Json.Serialization.JsonIgnore]
    public DateTime LastUpdateDate { get; set; }
}
