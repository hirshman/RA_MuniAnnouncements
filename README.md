# MuniAnnouncements_RA – "הבקשות שלי"

> מיקרו-שירות לאזור האישי של התושב – מרכז את כלל הבקשות שהוגשו לרשויות המקומיות בתצוגה אחודה.

---

## 🗂️ תוכן עניינים

1. [רקע ומטרה](#רקע-ומטרה)
2. [ארכיטקטורה](#ארכיטקטורה)
3. [מבנה הפרויקט](#מבנה-הפרויקט)
4. [User Stories & Tasks](#user-stories--tasks)
5. [שאלות הבהרה והנחות](#שאלות-הבהרה-והנחות)
6. [מודלי נתונים](#מודלי-נתונים)
7. [הוראות הרצה](#הוראות-הרצה)
8. [בדיקות יחידה](#בדיקות-יחידה)
9. [שימוש בכלי AI](#שימוש-בכלי-ai)

---

## רקע ומטרה

השירות `MuniAnnouncements_RA` מפותח כחלק מפלטפורמת אתרי האינטרנט של הרשויות המקומיות.  
מטרתו לרכז עבור התושב המחובר את כלל הבקשות שהגיש לרשויות השונות (הנחה בארנונה, תו חניה, דיווח על מפגע וכד') בתצוגה אחודה, כולל סטטוס עדכני, סימון פעולה נדרשת וחריגת SLA.

---

## ארכיטקטורה

```
┌─────────────────────────────────────────────────────────────┐
│                     Angular Frontend                        │
│              רכיב "הבקשות שלי" (my-requests)               │
└──────────────────────────┬──────────────────────────────────┘
                           │ JWT Bearer Token
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              MuniAnnouncements.Api  (.NET 8)                │
│         GET /api/my-requests  [Authorize]                   │
│         RequestsController → קריאת claim idNumber          │
└──────┬───────────────────────────────────────┬──────────────┘
       │                                       │
       ▼                                       ▼
┌──────────────────┐                ┌──────────────────────────┐
│  IFormsSystem    │                │  IServiceCatalog         │
│  Client (Mock)   │                │  Repository (Mock)       │
│                  │                │  ← MongoDB בעתיד         │
│  מחזיר RawRequest│                │  מחזיר ServiceCatalog    │
└──────┬───────────┘                └──────────────┬───────────┘
       │                                           │
       └──────────────┬────────────────────────────┘
                      ▼
          ┌───────────────────────┐
          │  NormalizationService │
          │  (MuniAnnouncements   │
          │       .Services)      │
          │                       │
          │ • שם שירות + רשות     │
          │ • תרגום סטטוס         │
          │ • actionRequired      │
          │ • isOverdue (SLA)     │
          │ • מיון + סינון        │
          │ • הסתרת ת"ז           │
          └───────────────────────┘
```

### עקרונות עיצוב

| עיקרון | יישום |
|--------|-------|
| **Microservice** | שירות עצמאי עם endpoint יחיד, ללא תלות ב-DB ישיר |
| **Dependency Injection** | כל התלויות מוזרקות דרך `IServiceCollection` |
| **Interfaces** | `IFormsSystemClient`, `IServiceCatalogRepository`, `INormalizationService` |
| **Mock → Real** | החלפת Mock במימוש אמיתי ללא שינוי קוד עסקי |
| **JWT Auth** | Middleware סטנדרטי, claim `idNumber`, מפתח סימטרי מה-config |
| **Layered Architecture** | Controller → Service → Repository/Client |

---

## מבנה הפרויקט

```
MuniAnnouncements/
├── MuniAnnouncements.Api/          # שכבת ה-API – Controllers, Program.cs, JWT
│   ├── Controllers/
│   │   └── RequestsController.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── MuniAnnouncements.Core/         # מודלים וממשקים משותפים
│   ├── Models/
│   │   ├── RawRequest.cs
│   │   ├── NormalizedRequest.cs
│   │   └── ServiceCatalogEntry.cs
│   └── Interfaces/
│       ├── IFormsSystemClient.cs
│       ├── IServiceCatalogRepository.cs
│       └── INormalizationService.cs
│
├── MuniAnnouncements.Services/     # לוגיקה עסקית + Mocks
│   ├── NormalizationService.cs
│   ├── Mocks/
│   │   ├── MockFormsSystemClient.cs
│   │   └── MockServiceCatalogRepository.cs
│
├── MuniAnnouncements.Tests/        # בדיקות יחידה (xUnit)
│   └── NormalizationServiceTests.cs
│
└── README.md
```

---

## User Stories & Tasks

### 🔧 Backend – C# / .NET 8

#### US-1 – צפייה בבקשות שהוגשו
> כתושב מחובר, אני רוצה לראות את כל הבקשות שהגשתי לרשויות השונות, כדי לעקוב אחר הסטטוס שלהן ממקום אחד.

| Task | תיאור | זמן | פרויקט |
|------|-------|-----|---------|
| **T1.1** | הגדרת מודלים: `RawRequest`, `NormalizedRequest`, `ServiceCatalogEntry` | 1h | Core |
| **T1.2** | ממשק `IFormsSystemClient` + `MockFormsSystemClient` עם Hard-Coded data | 1h | Core / Services |
| **T1.3** | ממשק `IServiceCatalogRepository` + `MockServiceCatalogRepository` עם Hard-Coded data | 1h | Core / Services |
| **T1.4** | `NormalizationService` – לוגיקה מלאה (סעיפים 4.1–4.7) | 3h | Services |
| **T1.5** | `RequestsController` – `GET /api/my-requests`, קריאת claim `idNumber` | 1h | Api |
| **T1.6** | רישום DI ב-`Program.cs` + JWT Middleware (מפתח סימטרי מה-config) | 1h | Api |

**סה"כ: 8h**

---

#### US-2 – הצגת סטטוס, פעולה נדרשת וחריגת SLA
> כתושב, אני רוצה לדעת אם נדרשת ממני פעולה ואם הבקשה חרגה מזמן הטיפול.

| Task | תיאור | זמן |
|------|-------|-----|
| **T2.1** | לוגיקת `actionRequired` – בדיקה מול `actionRequiredStatuses` | כלול ב-T1.4 |
| **T2.2** | לוגיקת `isOverdue` – ימים מ-`submittedAt` מול `slaDays`, רק לבקשות שאינן ב-`finalStatuses` | כלול ב-T1.4 |
| **T2.3** | מיון: `actionRequired` קודם, אחר כך `lastUpdate` DESC | כלול ב-T1.4 |
| **T2.4** | סטטוס לא מוכר → "בטיפול" + כתיבה ל-Log | כלול ב-T1.4 |
| **T2.5** | `formCode` לא בקטלוג → סינון מהתשובה + כתיבה ל-Log | כלול ב-T1.4 |

---

### 🎨 Frontend – Angular

#### US-3 – רכיב "הבקשות שלי" – תצוגה ראשית
> כתושב, אני רוצה לראות את הבקשות בממשק ברור, נגיש ורספונסיבי.

| Task | תיאור | זמן |
|------|-------|-----|
| **T3.1** | `MyRequestsComponent` – קריאה ל-`GET /api/my-requests` והצגת רשימה | 2h |
| **T3.2** | סימון בולט לבקשות עם `actionRequired: true` + CTA | 1h |
| **T3.3** | סימון ויזואלי לבקשות עם `isOverdue: true` | 0.5h |
| **T3.4** | מצב ריק – "עדיין לא הגשת בקשות" | 0.5h |
| **T3.5** | מצב שגיאה – הודעה ידידותית + כפתור "נסה שוב" | 0.5h |
| **T3.6** | RTL, עיצוב רספונסיבי, נגישות מלאה (ARIA, contrast, keyboard nav) | 1h |

**סה"כ: 5.5h**

---

### 🧪 בדיקות יחידה – xUnit

#### US-4 – כיסוי בדיקות לוגיקת הנרמול
> כמפתח, אני רוצה לוודא שלוגיקת הנרמול עובדת נכון בכל תרחיש קצה.

| Task | תיאור | זמן |
|------|-------|-----|
| **T4.1** | נרמול תקין – שם שירות, רשות וסטטוס מוחזרים נכון | 0.5h |
| **T4.2** | סטטוס לא מוכר → "בטיפול" + וידוא כתיבה ל-Log | 0.5h |
| **T4.3** | חריגת SLA – בקשה פתוחה מעל `slaDays` → `isOverdue: true` | 0.5h |
| **T4.4** | `formCode` לא בקטלוג → הבקשה לא מוחזרת ללקוח | 0.5h |
| **T4.5** | מיון – `actionRequired` מופיע לפני בקשה ללא, ללא תלות בתאריך | 0.5h |

**סה"כ: 2.5h**

---

### ⏱️ סיכום זמנים

| שכבה | זמן |
|------|-----|
| 🔧 Backend | 8h |
| 🎨 Frontend | 5.5h |
| 🧪 בדיקות | 2.5h |
| **סה"כ** | **16h** |

---

## שאלות הבהרה והנחות

### Q1 – אימות JWT
**שאלה:** האם קיים Identity Provider מרכזי (Keycloak / Azure AD) שמנפיק את הטוקן, או שיש לדמות אותו?  
**הנחה:** משתמשים ב-JWT סטנדרטי עם claim בשם `idNumber`. מימוש Middleware שמאמת חתימה עם מפתח סימטרי מה-config (`appsettings.json`).

### Q2 – isOverdue ובקשות סגורות
**שאלה:** האם בקשה ב-`finalStatuses` נחשבת "סגורה" לצורך חישוב SLA?  
**הנחה:** כן – רק בקשות שאינן ב-`finalStatuses` נבדקות לחריגת SLA.

### Q3 – פורמט תאריך
**שאלה:** האובייקט המנורמל מציג `"02/09/2026"` – האם הפורמט הוא DD/MM/YYYY?  
**הנחה:** כן, DD/MM/YYYY.

### Q4 – ריבוי רשויות
**שאלה:** האם תושב יכול להגיש אותו `formCode` לשתי רשויות שונות?  
**הנחה:** כן – כל `requestId` הוא ייחודי; ניתן להגיש אותו טופס לרשויות שונות.

---

## מודלי נתונים

### RawRequest (מערכת הטפסים)
```json
{
  "requestId": "F-88213",
  "formCode": "ARNONA_DISCOUNT",
  "authorityId": "6500",
  "applicantId": "012345678",
  "statusCode": "3",
  "submittedAt": "2026-09-02T10:15:00Z",
  "lastUpdate": "2026-09-20T08:00:00Z"
}
```

### ServiceCatalogEntry (MongoDB)
```json
{
  "formCode": "ARNONA_DISCOUNT",
  "serviceName": "הנחה בארנונה",
  "authorities": {
    "6500": {
      "authorityName": "עיריית חדרה",
      "statusMap": { "1": "נדרשים מסמכים", "2": "בטיפול", "3": "התקבלה", "4": "נדחתה", "5": "אושרה" },
      "actionRequiredStatuses": ["3"],
      "finalStatuses": ["4", "5"],
      "slaDays": 30
    }
  }
}
```

### NormalizedRequest (תשובה ללקוח)
```json
{
  "requests": [
    {
      "requestId": "F-88213",
      "serviceName": "הנחה בארנונה",
      "authorityName": "עיריית חדרה",
      "status": "נדרשים מסמכים",
      "actionRequired": true,
      "isOverdue": false,
      "submittedAt": "02/09/2026",
      "lastUpdate": "20/09/2026"
    }
  ]
}
```

> ⚠️ שדה `applicantId` (ת"ז) **אינו** מוחזר ללקוח בשום מקרה.

---

## הוראות הרצה

### דרישות מקדימות
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 / VS Code / Rider

### הרצת ה-API

```bash
cd MuniAnnouncements.Api
dotnet run
```

ה-API יהיה זמין בכתובת: `https://localhost:5001`

### קריאה לנקודת הקצה

```bash
# קבלת JWT תקף (ראה הגדרות JWT בהמשך)
curl -X GET https://localhost:5001/api/my-requests \
  -H "Authorization: Bearer <your_jwt_token>"
```

### הגדרות JWT ב-appsettings.json

```json
{
  "Jwt": {
    "Key": "<your_symmetric_secret_key_min_32_chars>",
    "Issuer": "MuniAnnouncements",
    "Audience": "MuniPortal"
  }
}
```

הטוקן חייב לכלול claim בשם `idNumber` עם מספר ת"ז של התושב.

### הרצת בדיקות

```bash
cd MuniAnnouncements.Tests
dotnet test
```

---

## בדיקות יחידה

הבדיקות מכסות את `NormalizationService` עם 5 תרחישים:

| בדיקה | תרחיש |
|-------|--------|
| T4.1 | נרמול תקין – שם שירות, רשות וסטטוס |
| T4.2 | סטטוס לא מוכר → "בטיפול" + Log |
| T4.3 | חריגת SLA → `isOverdue: true` |
| T4.4 | `formCode` לא בקטלוג → בקשה מסוננת |
| T4.5 | מיון – `actionRequired` קודם |

---

## שימוש בכלי AI

| כלי | שימוש |
|-----|-------|
| **Amazon Q Developer** | סיוע בתכנון ארכיטקטורה, יצירת מודלים, לוגיקת נרמול, בדיקות יחידה, תיעוד README |

> כל הקוד נבדק, הובן ואושר על ידי המפתח. הפתרון מוכן להצגה ולשינויים בזמן אמת בראיון.

---

*MuniAnnouncements_RA · מחלקת יישום רשויות מקומיות · יחידת השירותים הדיגיטליים · מערך הדיגיטל הלאומי*
