using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MuniAnnouncements.Core.Interfaces;
using MuniAnnouncements.Core.Models.Db;
using MuniAnnouncements.Core.Models.External;
using MuniAnnouncements.Services;

namespace MuniAnnouncements.Tests;

/// <summary>
/// בדיקות יחידה ללוגיקת הנרמול – T4.1 עד T4.5
/// </summary>
public class NormalizationServiceTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static AuthorityConfig DefaultAuthority() => new()
    {
        AuthorityName          = "עיריית חדרה",
        StatusMap              = new() { ["1"] = "נדרשים מסמכים", ["2"] = "בטיפול", ["4"] = "נדחתה" },
        ActionRequiredStatuses = ["1"],
        FinalStatuses          = ["4"],
        SlaDays                = 30
    };

    private static ServiceCatalogEntry DefaultCatalogEntry() => new()
    {
        FormCode    = "ARNONA_DISCOUNT",
        ServiceName = "הנחה בארנונה",
        Authorities = new() { ["6500"] = DefaultAuthority() }
    };

    private static RawRequest BaseRaw(string statusCode = "2", DateTime? submittedAt = null) => new()
    {
        RequestId   = "F-001",
        FormCode    = "ARNONA_DISCOUNT",
        AuthorityId = "6500",
        ApplicantId = "012345678",
        StatusCode  = statusCode,
        SubmittedAt = submittedAt ?? DateTime.UtcNow.AddDays(-5),
        LastUpdate  = DateTime.UtcNow.AddDays(-1)
    };

    private static NormalizationService Build(
        IFormsSystemClient formsClient,
        IServiceCatalogRepository catalogRepo)
        => new(formsClient, catalogRepo, NullLogger<NormalizationService>.Instance);

    // ── stub helpers ─────────────────────────────────────────────────────────

    private sealed class StubForms(IEnumerable<RawRequest> data) : IFormsSystemClient
    {
        public Task<IEnumerable<RawRequest>> GetRequestsByApplicantAsync(string _)
            => Task.FromResult(data);
    }

    private sealed class StubCatalog(ServiceCatalogEntry? entry) : IServiceCatalogRepository
    {
        public Task<ServiceCatalogEntry?> GetByFormCodeAsync(string _)
            => Task.FromResult(entry);
    }

    // ── T4.1 – נרמול תקין ────────────────────────────────────────────────────

    [Fact]
    public async Task T4_1_NormalRequest_ReturnsCorrectServiceAuthorityAndStatus()
    {
        var svc = Build(new StubForms([BaseRaw("2")]), new StubCatalog(DefaultCatalogEntry()));

        var results = await svc.NormalizeAsync("012345678");

        Assert.Single(results);
        Assert.Equal("הנחה בארנונה",   results[0].ServiceName);
        Assert.Equal("עיריית חדרה",    results[0].AuthorityName);
        Assert.Equal("בטיפול",         results[0].Status);
        Assert.False(results[0].ActionRequired);
        Assert.Equal("F-001",          results[0].RequestId);
        // ת"ז אסורה בתשובה
        Assert.DoesNotContain("012345678", results[0].RequestId);
    }

    // ── T4.2 – סטטוס לא מוכר → "בטיפול" ─────────────────────────────────────

    [Fact]
    public async Task T4_2_UnknownStatus_ReturnsBeTipul()
    {
        var svc = Build(new StubForms([BaseRaw("99")]), new StubCatalog(DefaultCatalogEntry()));

        var results = await svc.NormalizeAsync("012345678");

        Assert.Single(results);
        Assert.Equal("בטיפול", results[0].Status);
    }

    // ── T4.3 – חריגת SLA → isOverdue: true ──────────────────────────────────

    [Fact]
    public async Task T4_3_OverSla_IsOverdueTrue()
    {
        // submittedAt לפני 60 יום, SLA הוא 30 – צריך isOverdue=true
        var raw = BaseRaw("2", DateTime.UtcNow.AddDays(-60));
        var svc = Build(new StubForms([raw]), new StubCatalog(DefaultCatalogEntry()));

        var results = await svc.NormalizeAsync("012345678");

        Assert.True(results[0].IsOverdue);
    }

    // ── T4.4 – formCode לא בקטלוג → בקשה מסוננת ─────────────────────────────

    [Fact]
    public async Task T4_4_UnknownFormCode_RequestFiltered()
    {
        var svc = Build(new StubForms([BaseRaw()]), new StubCatalog(null));

        var results = await svc.NormalizeAsync("012345678");

        Assert.Empty(results);
    }

    // ── T4.5 – מיון: actionRequired קודם ─────────────────────────────────────

    [Fact]
    public async Task T4_5_Sorting_ActionRequiredFirst()
    {
        var rawAction = BaseRaw("1"); rawAction.RequestId = "F-ACTION"; rawAction.LastUpdate = DateTime.UtcNow.AddDays(-2);
        var rawNormal = BaseRaw("2"); rawNormal.RequestId = "F-NORMAL"; rawNormal.LastUpdate = DateTime.UtcNow.AddDays(-1);

        var svc = Build(new StubForms([rawNormal, rawAction]), new StubCatalog(DefaultCatalogEntry()));

        var results = await svc.NormalizeAsync("012345678");

        Assert.Equal("F-ACTION", results[0].RequestId);
        Assert.True(results[0].ActionRequired);
    }
}
