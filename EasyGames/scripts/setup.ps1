# Setup script for EasyGames (Windows PowerShell)
Set-Location -Path "$PSScriptRoot/.." # move to repo root

Write-Host "Restoring packages..."
dotnet restore EasyGames\EasyGames.csproj

Write-Host "Ensuring EF Core tools available (won't reinstall if present)"
try {
    dotnet tool list -g | Select-String dotnet-ef | Out-Null
} catch {
    # ignore
}

Write-Host "Applying migrations..."
cd EasyGames\EasyGames
# Apply migrations using local dotnet-ef if available
if (Get-Command dotnet-ef -ErrorAction SilentlyContinue) {
    dotnet ef database update --context ApplicationDbContext
} else {
    Write-Host "dotnet-ef not found. You can install it with: dotnet tool install --global dotnet-ef"
}

Write-Host "Setup complete. Run the app with: dotnet run --project EasyGames\EasyGames.csproj"
