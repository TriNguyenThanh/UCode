dotnet ef database drop -s .\Api\ -p .\Infrastructure\ -f
rm .\Infrastructure\Migrations\ -Recurse -Force

dotnet ef migrations add InitDb -s .\Api\ -p .\Infrastructure\
dotnet ef migrations add CreateView -s .\Api\ -p .\Infrastructure\

$MigrationName = "CreateView"
$MigrationDir  = ".\Infrastructure\Migrations"
$SourceFile    = ".\Infrastructure\EF\MigrationBuilders\CreateViewSource.txt"

if (!(Test-Path $SourceFile)) {
    Write-Error "Not found: $SourceFile"
    exit 1
}

$MigrationFile = Get-ChildItem $MigrationDir -Filter "*_${MigrationName}.cs" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $MigrationFile) {
    Write-Error "Not found migration '$MigrationName' in $MigrationDir"
    exit 1
}

Get-Content $SourceFile -Raw | Set-Content $MigrationFile.FullName -Encoding UTF8

Write-Host "Override file migration: $($MigrationFile.FullName) from $SourceFile"
