using MuniAnnouncements.Core.Models.Db;

namespace MuniAnnouncements.Core.Interfaces;

/// <summary>
/// ממשק לשליפת הגדרות שירות מ-MongoDB (ServiceCatalog collection)
/// </summary>
public interface IServiceCatalogRepository
{
    Task<ServiceCatalogEntry?> GetByFormCodeAsync(string formCode);
}
