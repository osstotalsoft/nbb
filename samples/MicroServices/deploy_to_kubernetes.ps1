$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

kubectl create -f $scriptDir\kubernetes.yaml -n nbb-samples
