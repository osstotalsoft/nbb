$level0Projects = @(
    "src\Application\NBB.Application.DataContracts\NBB.Application.DataContracts.csproj",
    "samples\MicroServices\NBB.Contracts\NBB.Contracts.PublishedLanguage\NBB.Contracts.PublishedLanguage.csproj",
    "samples\MicroServices\NBB.Contracts\NBB.Contracts.ReadModel\NBB.Contracts.ReadModel.csproj",
    "src\Core\NBB.Core.Abstractions\NBB.Core.Abstractions.csproj",
    "src\Core\NBB.Core.Configuration\NBB.Core.Configuration.csproj",
    "src\Core\NBB.Core.DependencyInjection\NBB.Core.DependencyInjection.csproj",
    "src\Core\NBB.Core.Effects\NBB.Core.Effects.csproj",
    "src\Core\NBB.Core.Evented.FSharp\NBB.Core.Evented.FSharp.fsproj",
    "src\Core\NBB.Core.FSharp\NBB.Core.FSharp.fsproj",
    "src\Core\NBB.Core.Pipeline\NBB.Core.Pipeline.csproj",
    "src\Correlation\NBB.Correlation\NBB.Correlation.csproj",
    "samples\MicroServices\NBB.Invoices\NBB.Invoices.PublishedLanguage\NBB.Invoices.PublishedLanguage.csproj",
    "src\Messaging\NBB.Messaging.DataContracts\NBB.Messaging.DataContracts.csproj",
    "samples\MicroServices\NBB.Payments\NBB.Payments.PublishedLanguage\NBB.Payments.PublishedLanguage.csproj",
    "src\EventStore\NBB.SQLStreamStore.Migrations\NBB.SQLStreamStore.Migrations.csproj",
    "samples\MultiTenancy\NBB.Todo.PublishedLanguage\NBB.Todo.PublishedLanguage.csproj",
    "src\Tools\Serilog\NBB.Tools.Serilog.Enrichers.ServiceIdentifier\NBB.Tools.Serilog.Enrichers.ServiceIdentifier.csproj",
    "src\Tools\Serilog\NBB.Tools.Serilog.OpenTelemetryTracingSink\NBB.Tools.Serilog.OpenTelemetryTracingSink.csproj"
)

$updatedCount = 0
foreach ($project in $level0Projects) {
    $fullPath = Join-Path "D:\Projects\GitHub\osstotalsoft\nbb" $project
    if (Test-Path $fullPath) {
        $content = Get-Content $fullPath -Raw
        $newContent = $content -replace '<TargetFramework>net9.0</TargetFramework>', '<TargetFramework>net10.0</TargetFramework>'
        Set-Content $fullPath -Value $newContent -NoNewline
        $updatedCount++
        Write-Host "✓ Updated: $project" -ForegroundColor Green
    } else {
        Write-Host "✗ Not found: $project" -ForegroundColor Red
    }
}
Write-Host "`n✓ Updated $updatedCount of $($level0Projects.Count) projects" -ForegroundColor Cyan
