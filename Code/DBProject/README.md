# Clinic Management System - .NET 8 Migration

## Overview
This is the migrated version of the Clinic Management System, converted from ASP.NET Web Forms 4.5.2 to .NET 8 using clean architecture principles.

## Architecture
The solution follows Clean Architecture with four layers:

```
ClinicManagement.sln
├── src/
│   ├── ClinicManagement.Domain          # Entities, interfaces, enums, exceptions
│   ├── ClinicManagement.Application     # Business logic, services, DTOs
│   ├── ClinicManagement.Infrastructure  # EF Core, repositories, data access
│   └── ClinicManagement.Web             # Razor Pages, UI layer
└── tests/
    └── ClinicManagement.UnitTests       # xUnit unit tests
```

## Prerequisites
- .NET 8 SDK
- SQL Server (or SQL Server Express)
- Visual Studio 2022 or VS Code

## Setup Instructions

### 1. Database Setup
Run the SQL scripts in the `Database Files` folder:
```sql
-- Run in order:
1. Schema.sql
2. Insertions.sql
```

### 2. Configuration
Update the connection string in `src/ClinicManagement.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.\\SQLEXPRESS;Initial Catalog=DBProject;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

### 3. Build and Run
```bash
cd src/ClinicManagement.Web
dotnet restore
dotnet build
dotnet run
```

## User Roles
- **Patient (Type=1)**: Can book appointments, view history, bills, notifications
- **Doctor (Type=2)**: Can manage appointments, update prescriptions, handle billing
- **Admin (Type=3)**: Can manage doctors, staff, view dashboard

## Key Features
- Login/Sign-up for patients
- Admin dashboard with statistics
- Doctor registration and management
- Staff management
- Appointment booking and management
- Treatment history tracking
- Billing management
- Patient notifications and feedback

## Migration Notes
See `docs/MIGRATION_NOTES.md` for detailed migration information.
