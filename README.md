<div align="center">

#  EstateFlow

### Estate Administration Management System

**C# • .NET 8 • WPF • ASP.NET Core • EF Core • Azure SQL • Azure Functions • SendGrid**

---

> **EstateFlow** is a desktop-based deceased-estate administration system designed to help Estate Controllers manage estates, correspondence, documents, progress, letters, and automated communications from one central workspace.

</div>

---

##  Project Overview

| Area | Description |
|---|---|
| **Frontend** | WPF desktop application using MVVM |
| **Backend** | ASP.NET Core Web API |
| **Database** | Azure SQL Database |
| **Documents** | Azure Blob Storage |
| **Automation** | Azure Functions |
| **Email** | SendGrid |
| **Authentication** | JWT + Two-Factor Authentication support |
| **Framework** | .NET 8 |
| **Primary Language** | C# |

---

##  Main Features

###  Estate Management

> Centralised management of deceased-estate matters.

- Create, view, update and delete estates.
- Each Estate Controller only sees estates assigned to their account.
- Store deceased details, estate file numbers and administration information.
- Display the estate's latest progress directly on the overview screen.

###  Executors & Beneficiaries

- Add Executors and Beneficiaries to an estate.
- Store email addresses for correspondence.
- Use recipient email details for automated reports and manual communication.

###  Progress Tracking

EstateFlow uses a flexible timeline rather than limiting progress to fixed Master's Office stages.

Examples of progress entries:

```text
Report sent to heirs
Spoke to executor regarding outstanding documents
Documents received
Vehicle transfer submitted
Awaiting response from financial institution
```

Progress entries can be **added, edited and deleted**, and the latest update is reflected on the estate overview.

###  Diary & Correspondence

The Diary acts as a working correspondence area for an estate.

```text
Diary Entry
   ↓
Letter-style preview
   ↓
Choose recipient
   ↓
Send email
```

Recipients can include:

- Estate Controller
- Executor
- Beneficiary
- Custom email address

Diary correspondence is sent through the Azure-hosted backend using **Azure Functions + SendGrid**.

###  Documents & Scans

- Upload estate documents.
- Store file metadata in Azure SQL.
- Store the physical files in Azure Blob Storage.
- Open/download documents.
- Delete documents.
- Automatically generate document names.

###  Vehicle / Traffic Department Letters

- Capture vehicle information under movable assets.
- Generate Traffic Department-related estate correspondence.
- Support the vehicle-transfer workflow for deceased estates.

###  Financial Institution Letters

- Capture financial institution / claim information.
- Generate letters using estate and institution data.
- Support correspondence with banks, insurers and other institutions.

###  Monthly Estate Reports

- Automated monthly estate updates.
- Reports can be sent to Executors and Beneficiaries.
- Azure Functions perform the scheduled processing.
- SendGrid handles email delivery.
- Report settings are managed per estate.

### Authentication & Security

- User registration and login.
- JWT authentication.
- Two-factor authentication support.
- User-specific estate isolation.
- Production secrets are stored in Azure configuration instead of GitHub.

---

#  Solution Structure

```text
EstateFlow
│
├── EstateFlow.Core
│   └── Shared models and DTOs
│
├── EstateFlow.Data
│   └── EF Core, DbContext and repositories
│
├── EstateFlow.Api
│   └── ASP.NET Core Web API
│
├── EstateFlow.Desktop
│   └── WPF desktop application using MVVM
│
└── EstateFlow.Functions
    └── Azure Functions for automation and email tasks
```

---

# ☁️ Architecture

```text
┌──────────────────────────────┐
│     EstateFlow.Desktop       │
│        WPF / MVVM            │
└──────────────┬───────────────┘
               │ HTTPS + JWT
               ▼
┌──────────────────────────────┐
│       EstateFlow.Api         │
│      ASP.NET Core API        │
│        Hosted in Azure       │
└───────┬───────────┬──────────┘
        │           │
        │           └──────────────► Azure Blob Storage
        │                            Estate documents
        ▼
   Azure SQL
   Estate data

        EstateFlow.Api
              │
              ▼
┌──────────────────────────────┐
│       Azure Functions        │
│  MonthlyReportFunction       │
│  DiaryReminderFunction       │
│  SendDiaryEmailFunction      │
└──────────────┬───────────────┘
               │
               ▼
           SendGrid
               │
               ▼
        Email recipients
```

> [!NOTE]
> The WPF Desktop client is configured to communicate with the hosted Azure API. A team member can normally run the Desktop app without having production Azure SQL, Blob Storage or SendGrid secrets on their machine.

---

#  Requirements

| Requirement | Recommended |
|---|---|
| Operating System | Windows 10 / Windows 11 |
| IDE | Visual Studio |
| SDK | .NET 8 SDK |
| Workload | .NET Desktop Development |
| Version Control | Git |
| Internet | Required for hosted backend access |

Optional for backend/cloud development:

- Azure account/access
- Azure Functions tooling
- Entity Framework tooling

---

#  Quick Start

## 1️⃣ Clone the repository

```powershell
git clone https://github.com/moah15/EstateFlowTest.git
cd EstateFlowTest
```

## 2️⃣ Open the solution

Open:

```text
EstateFlow.sln
```

in Visual Studio.

## 3️⃣ Restore NuGet packages

Visual Studio should restore packages automatically.

If needed:

```powershell
dotnet restore
```

## 4️⃣ Build the solution

In Visual Studio:

```text
Build → Rebuild Solution
```

or:

```powershell
dotnet build
```

## 5️⃣ Set the Startup Project

In **Solution Explorer**:

```text
Right-click EstateFlow.Desktop
→ Set as Startup Project
```

## 6️⃣ Run EstateFlow

Press:

```text
F5
```

or click the green **Start** button.

---

#  Demo / Test Login

> [!IMPORTANT]
> Use a dedicated demo account only. Do not put a real employee's password in this README.

```text
Email:    <demo@estateflow.local>
Password: <EstateFlowDemo123!>
```

A new demo account may initially show **no estates** until estates are created for that account.

---

# How to Use EstateFlow

## Login

1. Launch `EstateFlow.Desktop`.
2. Enter the test credentials.
3. Complete 2FA if enabled.
4. The Dashboard loads estates belonging to the logged-in Estate Controller.

## Create an Estate

```text
Dashboard
   ↓
New Estate / Add Estate
   ↓
Capture estate information
   ↓
Save
```

## Estate Detail Areas

| Section | Purpose |
|---|---|
| **General** | Key estate information + current progress |
| **Executors** | Executor records and email details |
| **Beneficiaries** | Beneficiary/heir records and email details |
| **Movable Assets** | Vehicles and movable estate assets |
| **Immovable Properties** | Property information |
| **Financial Claims** | Banks, policies and institution claims |
| **Progress** | Flexible estate-administration timeline |
| **Diary** | Notes, follow-ups and email correspondence |
| **Documents** | Upload/manage estate documents |
| **Letters** | Generate estate correspondence |

---

#  Progress Example

```text
21 Sep 2026
Report sent to heirs
Status: Complete

19 Sep 2026
Spoke to executor regarding outstanding documents
Status: In Progress

17 Sep 2026
Awaiting response from financial institution
Status: Pending
```

---

# ✉️ Diary Email Flow

```text
Select Diary Entry
       ↓
Review Letter Preview
       ↓
Choose Recipient
       ↓
Edit Subject if needed
       ↓
Send Email
```

The message is routed through:

```text
EstateFlow.Desktop
       ↓
EstateFlow.Api
       ↓
SendDiaryEmailFunction
       ↓
SendGrid
       ↓
Recipient Inbox
```

---

# ☁️ Azure Services

| Azure / Cloud Service | Purpose |
|---|---|
| **Azure App Service** | Hosts `EstateFlow.Api` |
| **Azure SQL Database** | Stores estate/application data |
| **Azure Blob Storage** | Stores estate documents |
| **Azure Functions** | Runs automated tasks and email workflows |
| **Application Insights** | Monitoring and telemetry |
| **SendGrid** | Email delivery |

---

#  Azure Functions

### MonthlyReportFunction

Sends scheduled estate updates to Executors and Beneficiaries with valid email addresses.

### DiaryReminderFunction

Processes diary reminders based on due dates and notification settings.

### SendDiaryEmailFunction

Receives diary-email requests from the main API and sends correspondence through SendGrid.

---

#  Configuration & Secrets

> [!CAUTION]
> Never commit production credentials or connection strings to GitHub.

Do **not** commit:

```text
Azure Storage account keys
Azure SQL passwords
SendGrid API keys
Azure Function keys
JWT signing secrets
Production connection strings
```

The hosted application reads production configuration from Azure Environment Variables.

### Main API App Service

Typical settings:

```text
AzureStorage__ConnectionString
AzureStorage__ContainerName
DiaryEmailFunctionUrl
```

### Function App

Typical settings:

```text
EstateFlowDb
SendGridApiKey
SendGridFromEmail
AzureWebJobsStorage
```

---

#  Running Desktop Only

This is the recommended setup for most team members.

```text
EstateFlow.Desktop
       │
       ▼
Hosted EstateFlow.Api
       │
       ├────► Azure SQL
       ├────► Azure Blob Storage
       └────► Azure Functions / SendGrid
```

Because the backend is already hosted, a coworker can usually:

```text
Clone → Restore → Build → Run Desktop → Login
```

without configuring production cloud secrets locally.

---

#  Running the API Locally

Only required when actively developing/debugging `EstateFlow.Api`.

Local API development may require local values for:

- Azure SQL connection
- Azure Blob Storage
- JWT/authentication settings
- Function URLs

Recommended approaches:

```text
appsettings.Development.json
.NET User Secrets
Environment variables
```

Do not add real credentials to `appsettings.json`.

---

# ⚙️ Running Azure Functions Locally

Local Function development may require:

```text
local.settings.json
```

Typical values:

```text
AzureWebJobsStorage
EstateFlowDb
SendGridApiKey
SendGridFromEmail
```

`local.settings.json` must remain excluded from Git.

---

#  Entity Framework Migrations

If database models change, open **Package Manager Console** and set:

```text
Default project: EstateFlow.Data
```

Create a migration:

```powershell
Add-Migration MigrationName -Project EstateFlow.Data -StartupProject EstateFlow.Api
```

Apply it:

```powershell
Update-Database -Project EstateFlow.Data -StartupProject EstateFlow.Api
```

> [!NOTE]
> Only create a migration when the database model actually changes.

---

#  Git Workflow

Before starting:

```powershell
git pull
```

Create a branch:

```powershell
git checkout -b your-name-feature
```

Example:

```powershell
git checkout -b mohamed-report-ui
```

Commit your changes:

```powershell
git add .
git commit -m "Improve report settings UI"
git push -u origin your-name-feature
```

> [!WARNING]
> Avoid force-pushing the shared `main` branch once multiple developers are actively working on the project.

---

#  Troubleshooting

## App builds but does not launch

Check:

```text
EstateFlow.Desktop is the Startup Project
.NET 8 SDK is installed
NuGet restore completed
```

Then run:

```powershell
dotnet restore
dotnet build
```

## Login works but no estates appear

This is expected for a new account. EstateFlow isolates estates by Estate Controller.

Create a new estate under that account.

## 401 Unauthorized

The JWT may have expired.

```text
Log out → Log back in
```

## Document upload/open fails

The hosted API requires valid Azure Blob Storage configuration:

```text
AzureStorage__ConnectionString
AzureStorage__ContainerName
```

Desktop-only users should not need to configure these locally.

## Diary email says Function is not configured

The **main `estateflow` App Service** requires:

```text
DiaryEmailFunctionUrl
```

This belongs to the API App Service — **not** the Function App.

## Emails do not arrive

Check:

```text
Recipient email address
SendGrid sender verification
Spam / Junk folder
Azure Function invocation logs
SendGrid API configuration
```

## GitHub blocks push with GH013

GitHub Push Protection detected a secret.

```text
Remove the secret from source/history
Keep it in Azure Environment Variables
Push again
```

Do not bypass protection for genuine production credentials.

---

#  Technology Stack

| Technology | Usage |
|---|---|
| **C#** | Main application language |
| **.NET 8** | Application framework |
| **WPF** | Desktop UI |
| **XAML** | WPF layout and styling |
| **ASP.NET Core** | REST API |
| **Entity Framework Core** | ORM/data access |
| **Azure SQL** | Relational database |
| **Azure Blob Storage** | Estate documents |
| **Azure Functions** | Automation |
| **SendGrid** | Email delivery |
| **GitHub** | Version control and collaboration |

---

#  Current Scope

EstateFlow currently focuses on the **internal Estate Controller workflow**.

Primary areas:

```text
Estate administration
Progress tracking
Diary correspondence
Document management
Generated letters
Executor / Beneficiary communication
Monthly reporting
Cloud-backed shared data
```

A separate Executor/Heir web portal may be considered as a future enhancement.

---

#  Team Setup Summary

```text
1. Clone the repository
2. Open EstateFlow.sln
3. Restore NuGet packages
4. Rebuild Solution
5. Set EstateFlow.Desktop as Startup Project
6. Press F5
7. Login using the TEST account
```

> The Desktop client communicates with the shared Azure-hosted backend, allowing multiple team members to work with the same cloud environment without committing production Azure secrets to GitHub.

---

<div align="center">

## EstateFlow

**Estate Administration Management System**

Built with **C# • .NET 8 • WPF • ASP.NET Core • Microsoft Azure**

</div>
