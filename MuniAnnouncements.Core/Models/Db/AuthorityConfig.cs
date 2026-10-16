namespace MuniAnnouncements.Core.Models.Db;

/// <summary>
/// הגדרות שירות לרשות מסוימת בתוך מסמך ServiceCatalog ב-MongoDB
/// </summary>
public class AuthorityConfig
{
    public string AuthorityName { get; set; } = string.Empty;
    // מיפוי קוד סטטוס → שם סטטוס כפי שהרשות הגדירה
    public Dictionary<string, string> StatusMap { get; set; } = new();
    // סטטוסים הדורשים פעולה מהתושב
    public List<string> ActionRequiredStatuses { get; set; } = new();
    // סטטוסים סופיים – לא נבדקים לחריגת SLA
    public List<string> FinalStatuses { get; set; } = new();
    public int SlaDays { get; set; }
}
