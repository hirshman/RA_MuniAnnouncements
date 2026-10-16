namespace MuniAnnouncements.Core.Models.Db;

/// <summary>
/// מסמך מתוך ה-Collection ServiceCatalog ב-MongoDB
/// </summary>
public class ServiceCatalogEntry
{
    public string FormCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    // מפתח: authorityId
    public Dictionary<string, AuthorityConfig> Authorities { get; set; } = new();
}
