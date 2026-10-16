using Microsoft.Extensions.Logging;
using MuniAnnouncements.Core.Interfaces;
using MuniAnnouncements.Core.Models.Response;

namespace MuniAnnouncements.Services;

/// <summary>
/// שירות הנרמול – ממיר בקשות גולמיות לאובייקט תשובה מנורמל ללקוח
/// </summary>
public class NormalizationService : INormalizationService
{
    private readonly IFormsSystemClient _formsClient;
    private readonly IServiceCatalogRepository _catalogRepository;
    private readonly ILogger<NormalizationService> _logger;

    public NormalizationService(
        IFormsSystemClient formsClient,
        IServiceCatalogRepository catalogRepository,
        ILogger<NormalizationService> logger)
    {
        _formsClient       = formsClient;
        _catalogRepository = catalogRepository;
        _logger            = logger;
    }

    public async Task<IEnumerable<NormalizedRequest>> NormalizeAsync(string idNumber)
    {
        var rawRequests = await _formsClient.GetRequestsByApplicantAsync(idNumber);
        var result      = new List<NormalizedRequest>();

        foreach (var raw in rawRequests)
        {
            // 4.5 – formCode לא קיים בקטלוג → סינון + Log
            var catalogEntry = await _catalogRepository.GetByFormCodeAsync(raw.FormCode);
            if (catalogEntry is null)
            {
                _logger.LogWarning("formCode {FormCode} לא נמצא בקטלוג – הבקשה {RequestId} מסוננת", raw.FormCode, raw.RequestId);
                continue;
            }

            // 4.1 – שם שירות + שם רשות
            if (!catalogEntry.Authorities.TryGetValue(raw.AuthorityId, out var authorityConfig))
            {
                _logger.LogWarning("authorityId {AuthorityId} לא נמצא עבור {FormCode} – הבקשה {RequestId} מסוננת", raw.AuthorityId, raw.FormCode, raw.RequestId);
                continue;
            }

            // 4.2 – תרגום סטטוס; סטטוס לא מוכר → "בטיפול" + Log
            if (!authorityConfig.StatusMap.TryGetValue(raw.StatusCode, out var statusName))
            {
                _logger.LogWarning("statusCode {StatusCode} לא מוכר עבור {FormCode}/{AuthorityId} – הבקשה {RequestId} תוצג כ'בטיפול'", raw.StatusCode, raw.FormCode, raw.AuthorityId, raw.RequestId);
                statusName = "בטיפול";
            }

            // 4.3 – actionRequired
            var actionRequired = authorityConfig.ActionRequiredStatuses.Contains(raw.StatusCode);

            // 4.4 – isOverdue: רק לבקשות שאינן בסטטוס סופי
            var isFinal  = authorityConfig.FinalStatuses.Contains(raw.StatusCode);
            var isOverdue = !isFinal && (DateTime.UtcNow - raw.SubmittedAt).TotalDays > authorityConfig.SlaDays;

            result.Add(new NormalizedRequest
            {
                RequestId     = raw.RequestId,
                ServiceName   = catalogEntry.ServiceName,
                AuthorityName = authorityConfig.AuthorityName,
                Status        = statusName,
                ActionRequired = actionRequired,
                IsOverdue     = isOverdue,
                // 4.7 – ApplicantId (ת"ז) אינו נכלל בתשובה
                // 4.6 – פורמט תאריך DD/MM/YYYY
                SubmittedAt   = raw.SubmittedAt.ToString("dd/MM/yyyy"),
                LastUpdate    = raw.LastUpdate.ToString("dd/MM/yyyy")
            });
        }

        // 4.6 – מיון: actionRequired קודם, אחר כך lastUpdate DESC
        return result
            .OrderByDescending(r => r.ActionRequired)
            .ThenByDescending(r => r.LastUpdate);
    }
}
