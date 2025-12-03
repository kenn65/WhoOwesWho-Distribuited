Write-Host "Seeding WSL with SQL backup files..."

# --- CONFIG ---
$windowsSource   = "D:\WhoOwesWhoAspire\WhoOwesWho\WhoOwesWho.AppHost\Setup\Databases"
$wslTarget       = "\\wsl$\Ubuntu\home\kenn\who-owes-who-backups"   # <-- CHANGE username + distro
# ---------------------------------

# Ensure target exists
if (!(Test-Path $wslTarget)) {
    New-Item -ItemType Directory -Force -Path $wslTarget | Out-Null
}

# Copy all backup files
Get-ChildItem "$windowsSource\*.bak" | ForEach-Object {
    $dest = Join-Path $wslTarget $_.Name
    Write-Host "Copying $($_.Name) ..."
    Copy-Item $_.FullName $dest -Force
}

Write-Host "WSL backup seeding complete!"
