using MuniAnnouncements.Core.Interfaces;
using MuniAnnouncements.Core.Models.Db;

namespace MuniAnnouncements.Services.Mocks;

/// <summary>
/// מימוש דמה של קטלוג השירותים – מחזיר נתוני Hard-Coded במקום MongoDB
/// </summary>
public class MockServiceCatalogRepository : IServiceCatalogRepository
{
    private static readonly Dictionary<string, ServiceCatalogEntry> _catalog = new()
    {
        ["ARNONA_DISCOUNT"] = new ServiceCatalogEntry
        {
            FormCode    = "ARNONA_DISCOUNT",
            ServiceName = "הנחה בארנונה",
            Authorities = new Dictionary<string, AuthorityConfig>
            {
                ["6500"] = new AuthorityConfig
                {
                    AuthorityName          = "עיריית חדרה",
                    StatusMap              = new() { ["1"] = "נדרשים מסמכים", ["2"] = "בטיפול", ["3"] = "התקבלה", ["4"] = "נדחתה", ["5"] = "אושרה" },
                    ActionRequiredStatuses = ["1"],
                    FinalStatuses          = ["4", "5"],
                    SlaDays                = 30
                }
            }
        },
        ["PARKING_PERMIT"] = new ServiceCatalogEntry
        {
            FormCode    = "PARKING_PERMIT",
            ServiceName = "תו חניה",
            Authorities = new Dictionary<string, AuthorityConfig>
            {
                ["6500"] = new AuthorityConfig
                {
                    AuthorityName          = "עיריית חדרה",
                    StatusMap              = new() { ["1"] = "בבדיקה", ["2"] = "בטיפול", ["3"] = "אושר", ["4"] = "נדחה" },
                    ActionRequiredStatuses = [],
                    FinalStatuses          = ["3", "4"],
                    SlaDays                = 14
                }
            }
        }
    };

    public Task<ServiceCatalogEntry?> GetByFormCodeAsync(string formCode)
    {
        _catalog.TryGetValue(formCode, out var entry);
        return Task.FromResult(entry);
    }
}
