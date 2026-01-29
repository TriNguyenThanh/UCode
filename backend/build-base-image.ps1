# Build base image cho tất cả .NET services
Write-Host "🔨 Building ucode-dotnet-base image..." -ForegroundColor Cyan

docker build -f docker/dotnet-base.Dockerfile -t ucode-dotnet-base:latest .

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Base image built successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📦 Image size:" -ForegroundColor Yellow
    docker images ucode-dotnet-base:latest
} else {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}
