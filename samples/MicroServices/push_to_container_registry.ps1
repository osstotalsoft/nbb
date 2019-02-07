$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$samplesDir = Split-Path -Parent $scriptDir
$solutionDir = Split-Path -Parent $samplesDir
$version = "1.0.0"
$containerRegistry = "YOUR_CONTAINER_REGISTRY_URL"
$containerRegistryUser = "YOUR_CONTAINER_REGISTRY_USER"
$containerRegistryPassword = "YOUR_CONTAINER_REGISTRY_PASSWORD"

#$images = docker-compose -f $scriptDir\docker-compose.yml images
$images = @(
    ("nbb.contracts.api"),
	("nbb.contracts.worker"),
	("nbb.contracts.migrations"),
	("nbb.invoices.api"),
	("nbb.invoices.worker"),
	("nbb.invoices.migrations"),
	("nbb.payments.api"),
    ("nbb.payments.worker"),
	("nbb.payments.migrations")
)

dotnet restore $solutionDir\NBB.sln
dotnet publish $solutionDir\NBB.sln -c Release -o ./obj/Docker/publish
docker-compose -f $scriptDir\docker-compose.yml build


$env:KUBECONFIG="$scriptDir\config"
docker login $containerRegistry -u $containerRegistryUser -p $containerRegistryPassword


Foreach ($image in $images)
{
	docker tag $image`:$version $containerRegistry/$image`:$version
	docker push $containerRegistry/$image`:$version
}
