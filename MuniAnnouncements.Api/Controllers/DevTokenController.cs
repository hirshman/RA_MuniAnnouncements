#if DEBUG
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace MuniAnnouncements.Api.Controllers;

/// <summary>
/// ⚠️ לפיתוח בלבד – לא זמין ב-Release/Production.
/// מייצר JWT Token לבדיקה ידנית של ה-API.
/// </summary>
[ApiController]
public class DevTokenController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public DevTokenController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// מחזיר JWT Token תקף לבדיקה – זמין רק בסביבת Development
    /// </summary>
    [HttpGet("api/dev-token")]
    public IActionResult GetDevToken([FromQuery] string idNumber = "012345678")
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:            _configuration["Jwt:Issuer"],
            audience:          _configuration["Jwt:Audience"],
            claims:            [new Claim("idNumber", idNumber)],
            expires:           DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            warning  = "⚠️ לפיתוח בלבד – אל תשתמש בטוקן זה בסביבת ייצור",
            idNumber,
            token    = tokenString,
            usage    = $"Authorization: Bearer {tokenString}"
        });
    }
}
#endif
