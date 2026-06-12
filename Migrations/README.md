# Database Migrations

To apply migrations and update the database, use the following commands:

1. Add a migration (if needed):
   ```bash
   dotnet ef migrations add MigrationName
   ```

2. Update the database:
   ```bash
   dotnet ef database update
   ```

Ensure the connection string in `appsettings.json` is correctly configured before running these commands.
