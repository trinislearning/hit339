# EasyGames - Development setup

This project uses EF Core migrations and automatically applies migrations at startup. Follow these steps after pulling the repo:

1. Restore packages

   ```
   dotnet restore
   ```

2. (Optional) Install EF Core CLI if you want to run migrations manually

   ```powershell
   dotnet tool install --global dotnet-ef
   ```

3. Run the app (recommended) — migrations will be applied automatically during startup when running in Development environment:

   ```
   dotnet run --project EasyGames\EasyGames.csproj
   ```

4. If you prefer to apply migrations explicitly:

   ```powershell
   cd EasyGames\EasyGames
   dotnet ef database update --context ApplicationDbContext
   ```

5. If you have a local `EasyGames.db` with incompatible schema, delete it before running the app so migrations can create the correct schema.

Notes
- Do NOT commit local SQLite DB files.
- Migration files are included under `EasyGames/Migrations` and should be committed.
- Program.cs already calls `db.Database.MigrateAsync()` to auto-apply migrations.

If you want I can also add a `scripts/setup.ps1` that runs restore and applies migrations automatically.