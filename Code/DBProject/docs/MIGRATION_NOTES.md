# Migration Notes

## What Was Migrated

### From ASP.NET Web Forms 4.5.2 to .NET 8

| Old Component | New Component |
|---------------|---------------|
| `.aspx` pages | Razor Pages (`.cshtml`) |
| Code-behind (`.aspx.cs`) | Page Models (`.cshtml.cs`) |
| Master pages (`.master`) | Layout pages (`_Layout.cshtml`) |
| `Global.asax` | `Program.cs` |
| `Web.config` | `appsettings.json` |
| `packages.config` | `PackageReference` in `.csproj` |
| ADO.NET (`SqlCommand`, `SqlDataAdapter`) | Entity Framework Core 8.0 |
| `System.Web` | ASP.NET Core equivalents |
| `Session["key"]` | `HttpContext.Session.GetInt32/SetInt32` |
| `Response.Redirect` | `RedirectToPage()` |
| `Request.Form` | Method parameters / model binding |
| ViewState | Eliminated (stateless Razor Pages) |
| Server controls (`GridView`, `Label`) | HTML + Bootstrap 5 |

## Key Differences from Web Forms

1. **No ViewState**: Razor Pages are stateless; data is re-fetched on each request
2. **Dependency Injection**: All services injected via constructor
3. **Async/Await**: All I/O operations are async
4. **Clean Architecture**: Business logic separated from UI
5. **EF Core**: Replaces ADO.NET stored procedure calls with LINQ queries
6. **Bootstrap 5**: Replaces old Bootstrap 3 assets

## Breaking Changes

1. **Session**: Uses `ISession` with typed extension methods
2. **Authentication**: Simple session-based auth (can be upgraded to ASP.NET Core Identity)
3. **Database**: EF Core requires migration from stored procedures to LINQ
4. **Stored Procedures**: Original app used stored procedures; migrated to EF Core LINQ queries

## Configuration Changes

- Connection string moved from `Web.config` to `appsettings.json`
- Logging configured via Serilog in `Program.cs`
- No more `system.web` configuration section

## Known Issues

1. Stored procedures from the original app are replaced with EF Core queries
2. The `ReputeIndex` calculation logic may need adjustment
3. Free slot generation logic is simplified - original used stored procedures

## Future Improvements

1. Implement ASP.NET Core Identity for proper authentication
2. Add JWT tokens for API support
3. Implement proper pagination for large datasets
4. Add caching for frequently accessed data (departments, etc.)
5. Implement real-time notifications with SignalR
