$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$nugetSource = "YOUR_NUGET_REPO_URL"
$nugetApiKey = "YOUR_NUGET_API_KEY"
$packageVersion = "3.2.1"
$config = "Release"

$projectFilePaths = @(
    ("\src\Application\NBB.Application.MediatR\NBB.Application.MediatR.csproj"),

	("\src\Core\NBB.Core.Abstractions\NBB.Core.Abstractions.csproj"),
	("\src\Core\NBB.Core.DependencyInjection\NBB.Core.DependencyInjection.csproj"),

	("\src\Correlation\NBB.Correlation\NBB.Correlation.csproj"),
	("\src\Correlation\NBB.Correlation.AspNet\NBB.Correlation.AspNet.csproj"),
	("\src\Correlation\NBB.Correlation.Serilog\NBB.Correlation.Serilog.csproj"),

	("\src\Data\NBB.Data.Abstractions\NBB.Data.Abstractions.csproj"),
    ("\src\Data\NBB.Data.EntityFramework\NBB.Data.EntityFramework.csproj"),
    ("\src\Data\NBB.Data.EventSourcing\NBB.Data.EventSourcing.csproj"),

    ("\src\Domain\NBB.Domain\NBB.Domain.csproj"),
    ("\src\Domain\NBB.Domain.Abstractions\NBB.Domain.Abstractions.csproj"),

	("\src\EventStore\NBB.EventStore\NBB.EventStore.csproj"),
	("\src\EventStore\NBB.EventStore.Abstractions\NBB.EventStore.Abstractions.csproj"),
	("\src\EventStore\NBB.EventStore.AdoNet\NBB.EventStore.AdoNet.csproj"),
	("\src\EventStore\NBB.EventStore.AdoNet.Migrations\NBB.EventStore.AdoNet.Migrations.csproj"),
	("\src\EventStore\NBB.EventStore.Host\NBB.EventStore.Host.csproj"),
	("\src\EventStore\NBB.EventStore.MessagingExtensions\NBB.EventStore.MessagingExtensions.csproj"),
	("\src\EventStore\NBB.GetEventStore\NBB.GetEventStore.csproj"),
	("\src\EventStore\NBB.SQLStreamStore\NBB.SQLStreamStore.csproj"),
	("\src\EventStore\NBB.SQLStreamStore.Migrations\NBB.SQLStreamStore.Migrations.csproj"),

	("\src\Mediator\NBB.Mediator.OpenFaaS\NBB.Mediator.OpenFaaS.csproj"),

    ("\src\Messaging\NBB.Messaging.Abstractions\NBB.Messaging.Abstractions.csproj"),
	("\src\Messaging\NBB.Messaging.DataContracts\NBB.Messaging.DataContracts.csproj"),
	("\src\Messaging\NBB.Messaging.Host\NBB.Messaging.Host.csproj"),
	("\src\Messaging\NBB.Messaging.InProcessMessaging\NBB.Messaging.InProcessMessaging.csproj"),
    ("\src\Messaging\NBB.Messaging.Kafka\NBB.Messaging.Kafka.csproj"),
	("\src\Messaging\NBB.Messaging.Nats\NBB.Messaging.Nats.csproj"),

	("\src\Resiliency\NBB.Resiliency\NBB.Resiliency.csproj"),

	("\src\Tools\Export\NBB.Exporter\NBB.Exporter.csproj"),
	("\src\Tools\Export\NBB.Exporter.Csv\NBB.Exporter.Csv.csproj"),
	("\src\Tools\Export\NBB.Exporter.Excel\NBB.Exporter.Excel.csproj")
)

Foreach ($projectFilePath in $projectFilePaths)
{
    $projectFile = Split-Path $projectFilePath -leaf
    #Write-Host "projectFile: $projectFile"

    $projectFolder =  Split-Path $projectFilePath -parent
    #Write-Host "projectFolder: $projectFolder"

    $packageName = $projectFile.Substring(0, $projectFile.LastIndexOf('.'))
    #Write-Host "packageName: $packageName"

	dotnet pack -c $config --include-symbols $scriptDir$projectFilePath
    #nuget add $scriptDir$projectFolder\bin\$config\$packageName.$packageVersion.symbols.nupkg -source $nugetSource
	dotnet nuget push $scriptDir$projectFolder\bin\$config\$packageName.$packageVersion.symbols.nupkg -k $nugetApiKey -s $nugetSource
}


$templatesNuspecFilePaths = @(
    ("\template\UnitTest\NBB.UnitTest.Template\NBB.UnitTest.nuspec")
)

Foreach ($templateNuspecFilePath in $templatesNuspecFilePaths)
{
    $nuspecFile = Split-Path $templateNuspecFilePath -leaf
    #Write-Host "projectFile: $nuspecFile"

    $packageName = $nuspecFile.Substring(0, $nuspecFile.LastIndexOf('.'))
    ##Write-Host "packageName: $packageName"

	nuget pack $scriptDir$templateNuspecFilePath
	dotnet nuget push .\$packageName.$packageVersion.nupkg -k $nugetApiKey -s $nugetSource
}
