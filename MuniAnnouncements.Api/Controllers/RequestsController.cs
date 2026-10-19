using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MuniAnnouncements.Core.Interfaces;

namespace MuniAnnouncements.Api.Controllers;

/// <summary>
/// Controller לניהול בקשות התושב – נקודת הכניסה הראשית של ה-API
/// </summary>
[ApiController]
[Authorize] // דרישה 1: נקודת קצה מאובטחת
public class RequestsController : ControllerBase
{
    private readonly INormalizationService _normalizationService;

    public RequestsController(INormalizationService normalizationService)
    {
        _normalizationService = normalizationService;
    }

    /// <summary>
    /// GET /api/my-requests – מחזיר את כל הבקשות המנורמלות של התושב המחובר
    /// </summary>
    /// <returns>אובייקט עם מערך requests מנורמל (ללא ת"ז)</returns>
    [HttpGet("api/my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        // דרישה 1: המשתמש מזוהה לפי הטוקן – קריאת claim idNumber (לא מפרמטר ב-URL)
        var idNumber = User.FindFirst("idNumber")?.Value;
        if (string.IsNullOrEmpty(idNumber))
            return Unauthorized("claim idNumber חסר בטוקן");

        // דרישות 2-5: פנייה למערכת הטפסים, נרמול, והחזרת תשובה
        var requests = await _normalizationService.NormalizeAsync(idNumber);
        return Ok(new { requests });
    }
}
