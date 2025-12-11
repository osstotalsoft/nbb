$level1Projects = @(
    "src\Application\NBB.Application.MediatR\NBB.Application.MediatR.csproj",
    "src\Application\NBB.Application.MediatR.Effects\NBB.Application.MediatR.Effects.csproj",
    "src\Data\NBB.Data.Abstractions\NBB.Data.Abstractions.csproj",
    "src\Domain\NBB.Domain.Abstractions\NBB.Domain.Abstractions.csproj",
    "src\EventStore\NBB.EventStore.Abstractions\NBB.EventStore.Abstractions.csproj",
    "src\Messaging\NBB.Messaging.Abstractions\NBB.Messaging.Abstractions.csproj",
    "src\Correlation\NBB.Correlation.AspNet\NBB.Correlation.AspNet.csproj",
    "src\Correlation\NBB.Correlation.Serilog\NBB.Correlation.Serilog.csproj",
    "src\Correlation\NBB.Correlation.Serilog.SqlServer\NBB.Correlation.Serilog.SqlServer.csproj",
    "src\MultiTenancy\NBB.MultiTenancy.Abstractions\NBB.MultiTenancy.Abstractions.csproj",
    "src\Core\NBB.Core.Effects.FSharp\NBB.Core.Effects.FSharp.fsproj",
    "src\Application\NBB.Application.Mediator.FSharp\NBB.Application.Mediator.FSharp.fsproj",
    "test\UnitTests\Core\NBB.Core.Configuration.Tests\NBB.Core.Configuration.Tests.csproj",
    "test\UnitTests\Core\NBB.Core.Pipeline.Tests\NBB.Core.Pipeline.Tests.csproj",
    "test\UnitTests\Tools\NBB.Tools.Serilog.Enrichers.ServiceIdentifier.Tests\NBB.Tools.Serilog.Enrichers.ServiceIdentifier.Tests.csproj"
)

$updatedCount = 0
foreach ($project in $level1Projects) {
    $fullPath = Join-Path "D:\Projects\GitHub\osstotalsoft\nbb" $project
    if (Test-Path $fullPath) {
        $content = Get-Content $fullPath -Raw
        $newContent = $content -replace '<TargetFramework>net9.0</TargetFramework>', '<TargetFramework>net10.0</TargetFramework>'
        if ($content -ne $newContent) {
            Set-Content $fullPath -Value $newContent -NoNewline
            $updatedCount++
            Write-Host "✓ Updated: $project" -ForegroundColor Green
        } else {
            Write-Host "⊘ Skipped (already net10.0): $project" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ Not found: $project" -ForegroundColor Red
    }
}
Write-Host "`n✓ Updated $updatedCount of $($level1Projects.Count) projects" -ForegroundColor Cyan
