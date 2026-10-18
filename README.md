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
├── MuniAnnouncements.Api/
│   ├── Controllers/
│   │   ├── RequestsController.cs
│   │   └── DevTokenController.cs    ← מייצר JWT לבדיקה (#if DEBUG בלבד)
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Program.cs
│   ├── appsettings.json
│   └── MuniAnnouncements.Api.http   ← קובץ בדיקה ידנית
│
├── MuniAnnouncements.Core/
│   ├── Models/
│   │   ├── External/
│   │   │   └── RawRequest.cs        ← מגיע ממערכת הטפסים
│   │   ├── Db/
│   │   │   ├── ServiceCatalogEntry.cs  ← מסמך MongoDB
│   │   │   └── AuthorityConfig.cs
│   │   └── Response/
│   │       └── NormalizedRequest.cs ← תשובה ללקוח
│   └── Interfaces/
│       ├── IFormsSystemClient.cs
│       ├── IServiceCatalogRepository.cs
│       └── INormalizationService.cs
│
├── MuniAnnouncements.Services/
│   ├── NormalizationService.cs
│   └── Mocks/
│       ├── MockFormsSystemClient.cs
│       └── MockServiceCatalogRepository.cs
│
├── MuniAnnouncements.Tests/
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

| Task | תיאור | זמן |
|------|-------|-----|
| **T2.1** | לוגיקת `actionRequired` | כלול ב-T1.4 |
| **T2.2** | לוגיקת `isOverdue` | כלול ב-T1.4 |
| **T2.3** | מיון: `actionRequired` קודם, אחר כך `lastUpdate` DESC | כלול ב-T1.4 |
| **T2.4** | סטטוס לא מוכר → "בטיפול" + Log | כלול ב-T1.4 |
| **T2.5** | `formCode` לא בקטלוג → סינון + Log | כלול ב-T1.4 |

---

### 🧪 בדיקות יחידה – xUnit

#### US-4 – כיסוי בדיקות לוגיקת הנרמול

| Task | תיאור | זמן |
|------|-------|-----|
| **T4.1** | נרמול תקין – שם שירות, רשות וסטטוס מוחזרים נכון | 0.5h |
| **T4.2** | סטטוס לא מוכר → "בטיפול" + וידוא Log | 0.5h |
| **T4.3** | חריגת SLA → `isOverdue: true` | 0.5h |
| **T4.4** | `formCode` לא בקטלוג → בקשה מסוננת | 0.5h |
| **T4.5** | מיון – `actionRequired` קודם | 0.5h |

**סה"כ: 2.5h**

---

## שאלות הבהרה והנחות

### Q1 – אימות JWT
**שאלה:** האם קיים Identity Provider מרכזי שמנפיק את הטוקן?  
**הנחה:** JWT סטנדרטי עם claim `idNumber`, מפתח סימטרי מה-config.

### Q2 – isOverdue ובקשות סגורות
**הנחה:** רק בקשות שאינן ב-`finalStatuses` נבדקות לחריגת SLA.

### Q3 – פורמט תאריך
**הנחה:** DD/MM/YYYY.

### Q4 – ריבוי רשויות
**הנחה:** כל `requestId` ייחודי; ניתן להגיש אותו טופס לרשויות שונות.

---

## מודלי נתונים

### RawRequest (מערכת הטפסים)
```json
{
  "requestId": "F-88213",
  "formCode": "ARNONA_DISCOUNT",
  "authorityId": "6500",
  "applicantId": "012345678",
  "statusCode": "1",
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
      "actionRequiredStatuses": ["1"],
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
- Visual Studio 2022 (מומלץ) / VS Code / Rider

---

### שלב 1 – הרצת ה-API

#### דרך Visual Studio
1. פתח את `MuniAnnouncements.sln`
2. בחר פרופיל הרצה מהתפריט הנפתח:
   - **`https`** – מומלץ, Kestrel עם SSL על `https://localhost:7224`
   - **`http`** – ללא SSL על `http://localhost:5255`
   - **`IIS Express`** – דרך IIS Express על `https://localhost:44336`
3. לחץ **F5** או **▶ Run**
4. הדפדפן יפתח אוטומטית ל-`/swagger`

#### דרך Terminal
```bash
cd MuniAnnouncements.Api
dotnet run
```
ה-API יהיה זמין ב: `https://localhost:7224`

---

### שלב 2 – קבלת JWT Token לבדיקה

> ⚠️ endpoint זה קומפל **בסביבת Debug בלבד** (`#if DEBUG`) – אינו קיים ב-Release/Production.

לאחר הרצת ה-API, פתח בדפדפן:
```
https://localhost:7224/api/dev-token?idNumber=012345678
```
התשובה תכיל את הטוקן המוכן לשימוש.

---

### שלב 3 – בדיקת ה-API (לאחר קבלת הטוקן)

ניתן לבחור באחת משתי האפשרויות:

#### אפשרות א' – דרך Swagger UI

1. פתח `https://localhost:7224/swagger`
2. קרא ל-`GET /api/dev-token` → העתק את ערך `token` מהתשובה
3. לחץ על כפתור **Authorize** 🔒
4. הזן: `Bearer <הטוקן שהעתקת>`
5. לחץ **Authorize** → **Close**
6. פתח `GET /api/my-requests` → **Try it out** → **Execute**

#### אפשרות ב' – דרך קובץ `.http` ב-Visual Studio

1. פתח את `MuniAnnouncements.Api.http`
2. לחץ **Send Request** מעל בקשת `GET /api/dev-token` – קבל טוקן
3. הדבק את הטוקן בשדה `@token`
4. לחץ **Send Request** מעל בקשת `GET /api/my-requests`

---

### הרצת צד הלקוח (Angular)

#### דרישות מקדימות
- [Node.js](https://nodejs.org) גרסה 18 ומעלה

#### הרצה
```bash
cd my-requests-ui
npm install
npm start
```
הרכיב יהיה זמין ב: `http://localhost:4200`

> ⚠️ יש להריץ את ה-API (שלב 1) לפני הרצת צד הלקוח.

> **טוקן בסביבת dev:** הרכיב מביא את הטוקן **אוטומטית** מ-`/api/dev-token` דרך `app.config.ts` – אין צורך להכניס טוקן ידנית ל-`localStorage`.

> **כתובת ה-API** מוגדרת ב-`src/environments/environment.ts` (ברירת מחדל: `https://localhost:7224`).

> **CORS:** ה-API מאפשר בקשות מ-`http://localhost:4200` בלבד (מוגדר ב-`appsettings.json`).

---

### הרצת בדיקות יחידה

#### אפשרות א' – דרך Terminal

```bash
cd MuniAnnouncements.Tests
dotnet test
```

תוצאה צפויה:
```
Passed!  - Failed: 0, Passed: 5, Skipped: 0, Total: 5
```

#### אפשרות ב' – דרך Visual Studio

1. פתח **Test Explorer**: `Test → Test Explorer` (או `Ctrl+E, T`)
2. לחץ **Run All Tests** (או `Ctrl+R, A`)
3. כל בדיקה שעברה מסומנת ✅ ירוק – לחיצה עליה מציגה את פרטי הריצה

---

## שימוש בכלי AI

| כלי | שימוש |
|-----|-------|
| **Amazon Q Developer** | סיוע בתכנון ארכיטקטורה, יצירת מודלים, לוגיקת נרמול, בדיקות יחידה, תיעוד README |

> כל הקוד נבדק, הובן ואושר על ידי המפתח. הפתרון מוכן להצגה ולשינויים בזמן אמת בראיון.

---

*MuniAnnouncements_RA · מחלקת יישום רשויות מקומיות · יחידת השירותים הדיגיטליים · מערך הדיגיטל הלאומי*
