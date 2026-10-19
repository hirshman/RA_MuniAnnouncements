using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MuniAnnouncements.Core.Interfaces;
using MuniAnnouncements.Services;
using MuniAnnouncements.Services.Mocks;

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════════════════════════
// CORS – מאפשר לרכיב Angular (localhost:4200) לפנות ל-API
// ══════════════════════════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
    options.AddPolicy("Client", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ══════════════════════════════════════════════════════════════════════════════
// Dependency Injection – רישום שירותים
// להחלפת Mock במימוש אמיתי: שנה רק את השורות הבאות
// ══════════════════════════════════════════════════════════════════════════════
builder.Services.AddScoped<IFormsSystemClient, MockFormsSystemClient>();       // דרישה 2: פנייה למערכת הטפסים
builder.Services.AddScoped<IServiceCatalogRepository, MockServiceCatalogRepository>(); // דרישה 3: פנייה ל-MongoDB
builder.Services.AddScoped<INormalizationService, NormalizationService>();     // דרישה 4: לוגיקת נרמול

builder.Services.AddControllers();

// ══════════════════════════════════════════════════════════════════════════════
// JWT Authentication – דרישה 1: נקודת קצה מאובטחת
// המשתמש מזוהה לפי claim "idNumber" בטוקן
// ══════════════════════════════════════════════════════════════════════════════
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ── Swagger עם תמיכה ב-JWT ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MuniAnnouncements API", Version = "v1" });

    // הוספת כפתור Authorize ב-Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "הזן JWT Token: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Middleware Pipeline ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
