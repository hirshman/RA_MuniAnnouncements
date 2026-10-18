using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MuniAnnouncements.Core.Interfaces;

namespace MuniAnnouncements.Api.Controllers;

[ApiController]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly INormalizationService _normalizationService;

    public RequestsController(INormalizationService normalizationService)
    {
        _normalizationService = normalizationService;
    }

    /// <summary>
    /// מחזיר את כל הבקשות המנורמלות של התושב המחובר
    /// </summary>
    [HttpGet("api/my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        // קריאת מספר ת"ז מה-claim בטוקן – לא מפרמטר ב-URL
        var idNumber = User.FindFirst("idNumber")?.Value;
        if (string.IsNullOrEmpty(idNumber))
            return Unauthorized("claim idNumber חסר בטוקן");

        var requests = await _normalizationService.NormalizeAsync(idNumber);
        return Ok(new { requests });
    }
}
