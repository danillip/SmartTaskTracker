# SmartTaskTracker

Simple ASP.NET Core MVC application for managing events and tasks.

## Running locally

```bash
dotnet restore
dotnet ef database update
dotnet run --project SmartTaskTracker
```

Admin account:
- **login**: `admin`
- **password**: `P@ssw0rd!`

Requires .NET 8 SDK and SQL Server.

Events can be exported to calendars via the **ICS** link. Admins manage events and tasks; users see only their assignments and can report on them.
